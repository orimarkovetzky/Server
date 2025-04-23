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

namespace FlowServer.DBServices
{
    public class MachineDBServices
    {

        public MachineDBServices()
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

            using (SqlConnection con = connect("igroup16_test1"))
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

        public List<Task> GetMachineQueue(int id)
        {
            var paramDic = new Dictionary<string, object>
    {
        { "@machineId", id }
    };

            List<Task> tasks = new List<Task>();

            using (SqlConnection con = connect("igroup16_test1"))
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

                    Task task = new Task(batchId, machineId, estStartTime, estEndTime, status);
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

            using (SqlConnection con = connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("UpdateMachineStatus", con, paramDic))
            {
                return cmd.ExecuteNonQuery();
            }
        }


        public Machine findMachine(int machineId)
        {
            var paramDic = new Dictionary<string, object>
    {
        { "@MachineID", machineId }
    };

            using (SqlConnection con = connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("FindMachineById", con, paramDic))
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

    }
}