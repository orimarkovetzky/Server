using System.Data.SqlClient;
using System.Data;
using FlowServer.Models;
using FlowServer.DBServices;
using Task = FlowServer.Models.Task;



    public class TaskDBServices
    {
        public SqlConnection connect(String conString)
        {

            // read the connection string from the configuration file
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("DefaultConnection");
            SqlConnection con = new SqlConnection(cStr);
            con.Open();
            return
                con;
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

        private SqlCommand CreateCommandWithTextQuery(string sqlQuery, SqlConnection con, Dictionary<string, object> paramDic)
        {
            SqlCommand cmd = new SqlCommand
            {
                Connection = con,
                CommandText = sqlQuery,
                CommandTimeout = 10, // optional
                CommandType = CommandType.Text // <-- this is the key difference
            };

            if (paramDic != null)
            {
                foreach (KeyValuePair<string, object> param in paramDic)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            return cmd;
        }

        public List<Task> ReadTasks()
        {
            SqlConnection con = connect("DefaultConnection");
            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("spReadTasks", con, null);
            SqlDataReader reader = cmd.ExecuteReader();
            List<Task> tasks = new List<Task>();
            while (reader.Read())
            {
                Task task = new Task
                {
                    batch = Batch.FindBatch((int)reader["batchId"]),
                    machine = Machine.FindMachine((int)reader["machineId"]),
                    estStartTime = (DateTime)reader["estStartTime"],
                    estEndTime = (DateTime)reader["estEndTime"],
                    actStartTime = (DateTime)reader["actStartTime"],
                    actEndTime = (DateTime)reader["actEndTime"],
                    status = reader["status"].ToString()
                };
                tasks.Add(task);
            }
            reader.Close();
            con.Close();
            return tasks;
        }
    }

