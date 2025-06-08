using System.Data.SqlClient;

namespace FlowServer.DBServices
{
    public class CustomerDBServices
    {
       
            public CustomerDBServices() //defualt constructor
            {
            }

            public SqlConnection connect(String conString)
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
        public int InsertCustomer(string name, SqlConnection con, SqlTransaction tx)
        {
            using var cmd = new SqlCommand("InsertCustomer", con, tx);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@customerName", name);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
    }
