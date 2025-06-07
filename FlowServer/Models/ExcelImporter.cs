using ClosedXML.Excel;
using FlowServer.DBServices;
using FlowServer.Models;
using System.Data.SqlClient;
using System.Globalization;

public class ExcelOrderImporter
{
    private readonly string _connectionString;

    public ExcelOrderImporter(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void ImportOrdersFromExcel(string excelPath)
    {
        var customerMap = new Dictionary<string, int>();
        var customerRefMap = new Dictionary<string, string>(); // name => refKey
        var tempCustomers = new List<string>();
        var tempOrders = new List<(string RefKey, DateTime OrderDate, DateTime SupplyDate)>();
        var tempBatches = new List<(string RefKey, int ProductId, int Quantity)>();

        using var workbook = new XLWorkbook(excelPath);
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            string customerIdStr = row.Cell(2).GetValue<string>()?.Trim() ?? "";
            string customerName = row.Cell(3).GetValue<string>()?.Trim() ?? "";
            string productIdStr = row.Cell(7).GetValue<string>()?.Trim() ?? "";
            string quantityStr = row.Cell(8).GetValue<string>()?.Trim() ?? "";

            if (!int.TryParse(productIdStr, out int productId))
                throw new Exception($"❌ Invalid productId in row {row.RowNumber()}, got '{productIdStr}'");
            if (!int.TryParse(quantityStr, out int quantity))
                throw new Exception($"❌ Invalid quantity in row {row.RowNumber()}, got '{quantityStr}'");

            var orderDateCell = row.Cell(4);
            DateTime orderDate;
            if (orderDateCell.DataType == XLDataType.DateTime)
                orderDate = orderDateCell.GetDateTime().Date;
            else if (!DateTime.TryParseExact(orderDateCell.GetValue<string>()?.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out orderDate))
                throw new Exception($"❌ Invalid orderDate in row {row.RowNumber()}, got '{orderDateCell.Value}'");

            var supplyDateCell = row.Cell(5);
            DateTime supplyDate;
            if (supplyDateCell.DataType == XLDataType.DateTime)
                supplyDate = supplyDateCell.GetDateTime().Date;
            else if (!DateTime.TryParseExact(supplyDateCell.GetValue<string>()?.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out supplyDate))
                throw new Exception($"❌ Invalid supplyDate in row {row.RowNumber()}, got '{supplyDateCell.Value}'");

            if (supplyDate < DateTime.Today)
                throw new Exception($"❌ Supply date {supplyDate:dd/MM/yyyy} is in the past (row {row.RowNumber()})");
            if (quantity <= 0)
                throw new Exception($"❌ Quantity must be greater than 0 (row {row.RowNumber()})");

            int customerId;
            string refKey;

            if (!string.IsNullOrWhiteSpace(customerIdStr))
            {
                if (!int.TryParse(customerIdStr, out customerId))
                    throw new Exception($"❌ Invalid customerID in row {row.RowNumber()}, got '{customerIdStr}'");
                if (!CustomerExists(customerId))
                    throw new Exception($"❌ Customer ID {customerId} not found (row {row.RowNumber()})");

                refKey = customerId.ToString();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(customerName))
                    throw new Exception($"❌ Row {row.RowNumber()} must have either customerID or customerName.");

                if (!customerMap.ContainsKey(customerName))
                {
                    customerMap[customerName] = -1; // reserve spot
                    customerRefMap[customerName] = $"NEW_{customerMap.Count}";
                    tempCustomers.Add(customerName);
                }

                refKey = customerRefMap[customerName];
            }

            tempOrders.Add((refKey, orderDate, supplyDate));
            tempBatches.Add((refKey, productId, quantity));
        }

        var finalCustomerIds = new Dictionary<string, int>();
        using var con = new SqlConnection(_connectionString);
        con.Open();
        using var tx = con.BeginTransaction();

        try
        {
            foreach (var name in tempCustomers)
            {
                int id = InsertCustomer(name, con, tx);
                finalCustomerIds[customerRefMap[name]] = id;
            }

            var orderIds = new Dictionary<string, int>();
            foreach (var order in tempOrders)
            {
                int custId = order.RefKey.StartsWith("NEW_") ? finalCustomerIds[order.RefKey] : int.Parse(order.RefKey);
                var newOrder = new Order { CustomerId = custId, OrderDate = order.OrderDate, SupplyDate = order.SupplyDate };
                int orderId = newOrder.InsertOrder(newOrder.CustomerId, newOrder.OrderDate, newOrder.SupplyDate, con, tx);
                orderIds[order.RefKey] = orderId;
            }

            foreach (var batch in tempBatches)
            {
                int orderId = orderIds[batch.RefKey];
                var newBatch = new Batch { OrderId = orderId, ProductId = batch.ProductId, Quantity = batch.Quantity };
                newBatch.InsertBatch(newBatch.OrderId,newBatch.ProductId,newBatch.Quantity,con, tx);
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private bool CustomerExists(int customerId)
    {
        using var con = new SqlConnection(_connectionString);
        con.Open();
        using var cmd = new SqlCommand("SELECT COUNT(*) FROM Customers WHERE customerID = @id", con);
        cmd.Parameters.AddWithValue("@id", customerId);
        return (int)cmd.ExecuteScalar() > 0;
    }

    private int InsertCustomer(string name, SqlConnection con, SqlTransaction tx)
    {
        using var cmd = new SqlCommand("InsertCustomer", con, tx);
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@customerName", name);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}

// Ensure your Order and Batch classes support overloads for Insert with (SqlConnection, SqlTransaction)
// And your stored procedure InsertCustomer returns SCOPE_IDENTITY()
