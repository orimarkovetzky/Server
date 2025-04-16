using System.Data.SqlClient;
using System.Data;
using FlowServer.Models;
using Task = FlowServer.Models.Task;

namespace FlowServer.DBServices
{
    public class QueueDBServices
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

        public List<Object> GetMachinePageData() //Returns all machines and their tasks for the machine page
        {
            try
            {
                // Retrieve all machines
                List<Machine> machines = Machine.ReadMachines();
                List<Object> machinePageData = new List<Object>();

                foreach (var machine in machines)
                {
                    // Get tasks for the current machine
                    List<Object> tasks = GetMachineTasks(machine.MachineId);

                    // Create an object with machine details and tasks
                    var machineData = new
                    {
                        MachineId = machine.MachineId,
                        MachineName = machine.MachineName,
                        MachineImage= machine.ImagePath,
                        Tasks = tasks
                    };

                    machinePageData.Add(machineData);
                }

                return machinePageData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching machine page data", ex);
            }



        }


        public List<object> GetMachineTasks(int machineId)
        {
            SqlConnection con = null;
            try
            {
                con = connect("igroup16_test1");
                string sqlQuery = "SPGetMachineTasks"; // stored procedure name
                Dictionary<string, object> paramDic = new Dictionary<string, object>
        {
            { "@machineID", machineId }
        };

                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral(sqlQuery, con, paramDic))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<object> tasks = new List<object>();
                        var counter = 1;
                        while (reader.Read())
                        {
                           
                            var taskObj = new
                            {
                                id = counter++,
                                name = reader["name"].ToString(),
                                units = Convert.ToInt32(reader["units"]),
                                processTime = Convert.ToInt32(reader["processTime"]),
                                progress = reader["progress"] != DBNull.Value ? float.Parse(reader["progress"].ToString()) : 0f,
                                inQueue = Convert.ToInt32(reader["inQueue"]) == 1,
                                type = Convert.ToInt32(reader["type"]),
                                color = reader["color"].ToString()
                            };

                            tasks.Add(taskObj);
                        }
                        return tasks;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }
        }

        public List<Task> GetMachineQueue(int id) //Not for use
        {
            {
                SqlConnection con = null;
                try
                {
                    con = connect("igroup16_test1");
                    string sqlQuery = "select * from MachineBatch where machineId=@machineId and status in ('Pending','Proccesing') order by status desc, startTimeEst";
                    Dictionary<string, object> paramDic = new Dictionary<string, object>
                {
                    { "@machineId",id }
                };

                    using (SqlCommand cmd = CreateCommandWithTextQuery(sqlQuery, con, paramDic))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<Task> tasks = new List<Task>();
                            while (reader.Read())
                            {
                                int batchId = Convert.ToInt32(reader["batchId"]);
                                int machineId = Convert.ToInt32(reader["machineId"]);
                                  DateTime estStartTime = reader["startTimeEst"] == DBNull.Value 
                        ? DateTime.MinValue.Date 
                        : Convert.ToDateTime(reader["startTimeEst"]);

                    DateTime estEndTime = reader["endTimeEst"] == DBNull.Value 
                        ? DateTime.MinValue.Date 
                        : Convert.ToDateTime(reader["endTimeEst"]);

                                string status = reader.GetString("status");
                                Task task = new Task(batchId, machineId,estStartTime, estEndTime, status);
                                tasks.Add(task);
                            }
                            return tasks;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (con != null)
                    {
                        con.Close();
                    }
                }
            }
        }
        public List<Batch> GetBatchesByStatus(string status)
        {
            SqlConnection con = null;
            try
            {
                con = connect("igroup16_test1");

                Dictionary<string, object> paramDic = new Dictionary<string, object>
        {
            { "@Status", status }
        };

                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetBatchesByStatus", con, paramDic))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<Batch> batches = new List<Batch>();

                        while (reader.Read())
                        {
                            Batch batch = new Batch
                            {
                                BatchId = reader.GetInt32(reader.GetOrdinal("batchId")),
                                OrderId = reader.GetInt32(reader.GetOrdinal("orderId")),
                                ProductId = reader.GetInt32(reader.GetOrdinal("productId")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                                Status = reader.GetString(reader.GetOrdinal("status"))
                            };

                            batches.Add(batch);
                        }

                        return batches;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

    }


}
