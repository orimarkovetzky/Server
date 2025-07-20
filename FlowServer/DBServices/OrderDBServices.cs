using System.Data.SqlClient;
using System.Dynamic;
using FlowServer.Models;


namespace FlowServer.DBServices
{
    public class OrderDBServices
    {
        public SqlConnection Connect(String conString)
        {
            // read the connection string from the configuration file
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("DefaultConnection");
            SqlConnection con = new SqlConnection(cStr);
            con.Open();
            return con;
        }

        private SqlCommand CreateCommandWithStoredProcedureGeneral(String spName, SqlConnection con, Dictionary<string, object> paramDic)
        {
            SqlCommand cmd = new SqlCommand(); // create the command object
            cmd.Connection = con;              // assign the connection to the command object
            cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 
            cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds
            cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

            if (paramDic != null)
                foreach (KeyValuePair<string, object> param in paramDic)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

            return cmd;
        }

        public int InsertOrder(int customerId, DateTime orderDate, DateTime supplyDate, SqlConnection con, SqlTransaction tx)
        {
            using var cmd = new SqlCommand("InsertOrder", con, tx);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@customerID", customerId);
            cmd.Parameters.AddWithValue("@orderDate", orderDate);
            cmd.Parameters.AddWithValue("@supplyDate", supplyDate);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }



        public List<Order> GetAllOrders()
        {
            // temporary lookup by OrderId
            var dict = new Dictionary<int, Order>();

            using (SqlConnection con = Connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetAllOrders", con, null))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    int orderId = r.GetInt32(r.GetOrdinal("orderID"));
                    int customerId = r.GetInt32(r.GetOrdinal("customerID"));
                    DateTime od = r.GetDateTime(r.GetOrdinal("orderDate"));
                    DateTime sd = r.GetDateTime(r.GetOrdinal("supplyDate"));
                    string customerName = r.GetString(r.GetOrdinal("customerName"));

                    // get or create the Order
                    if (!dict.TryGetValue(orderId, out var order))
                    {
                        order = new Order(orderId, customerId, od, sd,customerName)
                        {
                            Batches = new List<Batch>()
                        };
                        dict[orderId] = order;
                    }

                    // read the Batch fields
                    var batch = new Batch
                    {
                        BatchId = r.GetInt32(r.GetOrdinal("batchID")),
                        OrderId = orderId,
                        ProductId = r.GetInt32(r.GetOrdinal("productID")),
                        Quantity = r.GetInt32(r.GetOrdinal("quantity")),
                        Status = r.GetString(r.GetOrdinal("status"))
           
                    };

                    order.Batches.Add(batch);
                }
            }

            // return the grouped orders
            return dict.Values.ToList();
        }

    }

}