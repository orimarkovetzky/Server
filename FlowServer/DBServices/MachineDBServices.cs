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


        public List<Machine> ReadMachines()
        {
            SqlConnection con = null;
            List<Machine> machines = new List<Machine>();
            try
            {
                con = connect("igroup16_test1");
                String selectSTR = "SELECT * FROM Machines";
                SqlCommand cmd = CreateCommandWithTextQuery(selectSTR, con, null);
                SqlDataReader rdr = cmd.ExecuteReader();
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
                return machines;
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

        public List<Task> GetMachineQueue(int id) 
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
                                Task task = new Task(batchId, machineId, estStartTime, estEndTime, status);
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

        public int UpdateMachineStatus(int machineId, int newStatus)
        {
            SqlConnection con = null;
            try
            {
                con = connect("igroup16_test1");
                string updateStr = "UPDATE Machines SET status = @status WHERE machineId = @id";

                Dictionary<string, object> paramDic = new Dictionary<string, object>
        {
            { "@status", newStatus },
            { "@id", machineId }
        };

                SqlCommand cmd = CreateCommandWithTextQuery(updateStr, con, paramDic);
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected;
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

        public MachineDBServices()
        {

        }

        public Machine findMachine(int machineId)
        {

            SqlConnection con = null;
            try
            {
                con = connect("igroup16_test1");
                string selectSTR = $"SELECT * FROM Machines WHERE machineId = @MachineID";
                var paramDic = new Dictionary<string, object> { { "@MachineID", machineId } };

                SqlCommand cmd = CreateCommandWithTextQuery(selectSTR, con, paramDic);
                SqlDataReader rdr = cmd.ExecuteReader();

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

                return null; // Not found
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
}