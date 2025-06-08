using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Security.Cryptography;
using System.Xml.Linq;
using FlowServer.Models;
using Task = FlowServer.Models.Task;
using MachineCard = FlowServer.Models.MachineCard;
using System.Dynamic;

namespace FlowServer.DBServices
{
    public class MachineDBServices
    {

        public MachineDBServices()
        {

        }

        public SqlConnection Connect(String conString)
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
        public List<Machine> ReadMachines()
        {
            List<Machine> machines = new List<Machine>();

            using (SqlConnection con = Connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetAllMachines", con, null))
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    Machine m = new Machine
                    {
                        MachineId = Convert.ToInt32(rdr["MachineId"]),
                        MachineName = rdr["MachineName"].ToString(),
                        ImagePath = rdr["imagePath"].ToString(),
                        MachineType = Convert.ToInt32(rdr["MachineType"]),
                        SetupTime = Convert.ToSingle(rdr["SetupTime"]),
                        Status = Convert.ToInt32(rdr["Status"])
                    };
                    machines.Add(m);
                }
            }

            return machines;
        }

        public bool AddMachine(string machineName, int machineType, double setupTime, string imagePath = null)
        {
            using (SqlConnection con = Connect("igroup16_test1"))
            {
                // prepare parameters for the SP
                var parameters = new Dictionary<string, object>
        {
            { "@MachineName", machineName },
            { "@MachineType", machineType },
            { "@SetupTime", setupTime },
            { "@Status", 0 },                            // default status
            { "@ImagePath", (object)imagePath ?? DBNull.Value }
        };

                // call the stored procedure
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("AddMachine", con, parameters))
                {
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;  // true if insert succeeded
                }
            }
        }
        public bool DeleteMachine(string machineName)
        {
            using (SqlConnection con = Connect("igroup16_test1"))
            {
                var parameters = new Dictionary<string, object>
        {
            
            { "@MachineName", machineName }
        };

                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("DeleteMachineByName", con, parameters))
                {
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public List<Task> GetMachineQueue(int id)
        {
            var paramDic = new Dictionary<string, object>
    {
        { "@machineId", id }
    };

            List<Task> tasks = new List<Task>();

            using (SqlConnection con = Connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetMachineQueue", con, paramDic))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int batchId = Convert.ToInt32(reader["batchId"]);
                    int machineId = Convert.ToInt32(reader["machineId"]);

                    DateTime estStartTime = reader["startTimeEst"] == DBNull.Value
                        ? DateTime.MinValue
                        : Convert.ToDateTime(reader["startTimeEst"]);

                    DateTime estEndTime = reader["endTimeEst"] == DBNull.Value
                        ? DateTime.MinValue
                        : Convert.ToDateTime(reader["endTimeEst"]);

                    string status = reader["status"].ToString();
                    int processTime= Convert.ToInt32(reader["batchId"]);

                    Task task = new Task(batchId, machineId, estStartTime, estEndTime,status,processTime);
                    tasks.Add(task);
                }
            }

            return tasks;
        }
        public int UpdateMachineStatus(int machineId, int newStatus)
        {
            var paramDic = new Dictionary<string, object>
            {
                { "@id", machineId },
                { "@status", newStatus }

            };

            using (SqlConnection con = Connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("UpdateMachineStatus", con, paramDic))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public List<MachineCard> GetMachineCards()
        {
            var machineCards = new List<MachineCard>();

            List<Machine> machines = ReadMachines(); 

            foreach (var machine in machines)
            {
                var paramDic = new Dictionary<string, object>
        {
            { "@machineId", machine.MachineId }
        };

                string currentProduct = null;
                string nextProduct = null;
                int timeRemainingSeconds = 0;
                bool isDelayed = false;
                int progress = 0;

                using (SqlConnection con = Connect("igroup16_test1"))
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetTop2TasksByMachine", con, paramDic))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int taskIndex = 0;
                    while (reader.Read() && taskIndex < 2)
                    {
                        string productName = reader["productName"].ToString();
                        string taskStatus = reader["TaskStatus"].ToString();
                        DateTime? startTime = reader["startTime"] != DBNull.Value ? Convert.ToDateTime(reader["startTime"]) : (DateTime?)null;
                        DateTime? endTimeEst = reader["endTimeEst"] != DBNull.Value ? Convert.ToDateTime(reader["endTimeEst"]) : (DateTime?)null;
                        double? processTime = reader["processTime"] != DBNull.Value ? Convert.ToDouble(reader["processTime"]) : (double?)null;

                        if (taskIndex == 0) // First task = current product
                        {
                            currentProduct = productName;

                            // Calculate time remaining
                            if (endTimeEst.HasValue)
                            {
                                var remaining = endTimeEst.Value - DateTime.Now;
                                if (remaining.TotalSeconds < 0)
                                {
                                    isDelayed = true;
                                    timeRemainingSeconds = 0;
                                }
                                else
                                {
                                    timeRemainingSeconds = (int)remaining.TotalSeconds;
                                }
                            }

                            // Calculate progress
                            if (startTime.HasValue && processTime.HasValue && processTime.Value > 0)
                            {
                                var minutesPassed = (DateTime.Now - startTime.Value).TotalMinutes;
                                progress = (int)((minutesPassed / processTime.Value) * 100);

                                if (progress < 0) progress = 0;
                                if (progress > 100) progress = 100;
                            }
                        }
                        else if (taskIndex == 1) // Second task = next product
                        {
                            nextProduct = productName;
                        }

                        taskIndex++;
                    }
                }

                string status = machine.Status switch
                {
                    0 => "out_of_order",
                    1 => "ready",
                    2 => "maintenance",
                    3 => "running",
                    _ => "unknown"
                };

                var machineCard = new MachineCard(
                    machine.MachineId,
                    $"{machine.MachineName}",
                    machine.MachineType,
                    status,
                    currentProduct,
                    nextProduct,
                    progress,
                    timeRemainingSeconds,
                    isDelayed
                );

                machineCards.Add(machineCard);
            }

            return machineCards;
        }





        public Machine FindMachine(int machineId)
        {
            var paramDic = new Dictionary<string, object>
    {
        { "@MachineID", machineId }
    };

            using (SqlConnection con = Connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetMachineById", con, paramDic))
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    return new Machine()
                    {
                        MachineId = Convert.ToInt32(rdr["MachineID"]),
                        MachineType = Convert.ToInt32(rdr["MachineType"]),
                        MachineName = rdr["MachineName"].ToString(),
                        SetupTime = Convert.ToDouble(rdr["SetupTime"]),
                        Status = Convert.ToInt32(rdr["status"])
                    };
                }

                return null; //If machine not found
            }
        }

        public List<dynamic> GetMachineTasksOverview()
        {
            var result = new List<dynamic>();
            var groupedTasks = new Dictionary<int, List<dynamic>>();

            using (SqlConnection con = Connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetTop2TasksByMachine", con, null))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int machineId = reader["machineId"] != DBNull.Value ? Convert.ToInt32(reader["machineId"]) : 0;
                    string productName = reader["productName"] != DBNull.Value ? reader["productName"].ToString() : null;
                    DateTime? startTimeEst = reader["startTimeEst"] != DBNull.Value ? Convert.ToDateTime(reader["startTimeEst"]) : (DateTime?)null;
                    int processTime = reader["processTime"] != DBNull.Value ? Convert.ToInt32(reader["processTime"]) : 0;
                    DateTime? start = reader["startTime"] != DBNull.Value ? Convert.ToDateTime(reader["startTime"]) : (DateTime?)null;
                    int batchId = Convert.ToInt32(reader["batchId"]);
                    string status = reader["status"].ToString();

                    dynamic task = new ExpandoObject();
                    task.Name = productName;
                    task.EstimatedStart = startTimeEst;
                    task.StartTime = start;
                    task.Duration = processTime;
                    task.BatchId= batchId;
                    task.Status = status;

                    if (!groupedTasks.ContainsKey(machineId))
                        groupedTasks[machineId] = new List<dynamic>();

                    groupedTasks[machineId].Add(task);
                }
            }

            foreach (var entry in groupedTasks)
            {
                int machineId = entry.Key;
                var taskList = entry.Value;

                dynamic machineInfo = new ExpandoObject();
                machineInfo.Id = machineId;
                machineInfo.CurrentProduct = taskList.Count > 0 ? taskList[0].Name : null;
                machineInfo.StartTimeEst = taskList.Count > 0 ? taskList[0].EstimatedStart : (DateTime?)null;
                machineInfo.StartTime = taskList.Count > 0 ? taskList[0].StartTime : (DateTime?)null;
                machineInfo.ProcessTime = taskList.Count > 0 ? taskList[0].Duration : 0;
                machineInfo.NextProduct = taskList.Count > 1 ? taskList[1].Name : null;
                machineInfo.CurrentBatchId = taskList.Count > 0 ? taskList[0].BatchId : null;
                machineInfo.Status = taskList.Count > 0 ? taskList[0].Status : null;

                result.Add(machineInfo);
            }

            return result;
        }

        public int GetNonOperationalMachineCount()
        {
            using (SqlConnection con = Connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetNonOperationalMachineCount", con, null))
            {
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                        return Convert.ToInt32(rdr["NonOperationalCount"]);
                }
            }

            return 0;
        }

    }
}