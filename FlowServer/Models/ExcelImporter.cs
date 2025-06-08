using ClosedXML.Excel;
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
        var tempRows = new List<(string RefKey, string CustomerName, DateTime OrderDate, DateTime SupplyDate, int ProductId, int Quantity)>();

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

            string refKey;
            if (!string.IsNullOrWhiteSpace(customerIdStr))
            {
                if (!int.TryParse(customerIdStr, out int custId))
                    throw new Exception($"❌ Invalid customerID in row {row.RowNumber()}, got '{customerIdStr}'");
                if (!CustomerExists(custId))
                    throw new Exception($"❌ Customer ID {custId} not found (row {row.RowNumber()})");
                refKey = custId.ToString();
                customerMap[refKey] = custId;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(customerName))
                    throw new Exception($"❌ Row {row.RowNumber()} must have either customerID or customerName.");
                refKey = $"NEW_{customerName.ToLower()}";
                if (!customerMap.ContainsKey(refKey))
                    customerMap[refKey] = -1; // placeholder
            }

            tempRows.Add((refKey, customerName, orderDate, supplyDate, productId, quantity));
        }

        using var con = new SqlConnection(_connectionString);
        con.Open();
        using var tx = con.BeginTransaction();

        try
        {
            // Insert new customers
            foreach (var key in customerMap.Keys.ToList())
            {
                if (key.StartsWith("NEW_"))
                {
                    string name = key.Substring(4);
                    int newId = InsertCustomer(name, con, tx);
                    customerMap[key] = newId;
                }
            }

            // Group by logic depending on whether new or existing customer
            var orderMap = new Dictionary<string, int>();
            foreach (var row in tempRows)
            {
                int customerId = customerMap[row.RefKey];
                string orderKey = row.RefKey.StartsWith("NEW_")
     ? $"{row.RefKey}|{row.OrderDate:dd-MM-yyyy}|{row.SupplyDate:dd-MM-yyyy}"
     : $"{customerId}|{row.OrderDate:dd-MM-yyyy}|{row.SupplyDate:dd-MM-yyyy}";
            
                if (!orderMap.ContainsKey(orderKey))
                {
                    var newOrder = new Order { CustomerId = customerId, OrderDate = row.OrderDate, SupplyDate = row.SupplyDate };
                    int orderId = newOrder.InsertOrder(customerId, row.OrderDate, row.SupplyDate, con, tx);
                    orderMap[orderKey] = orderId;
                }

                int assignedOrderId = orderMap[orderKey];
                var newBatch = new Batch { OrderId = assignedOrderId, ProductId = row.ProductId, Quantity = row.Quantity };
                newBatch.InsertBatch(newBatch.OrderId, newBatch.ProductId, newBatch.Quantity, con, tx);
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
