using System.Data.SqlClient;
using System.Data;
using FlowServer.Models;
using FlowServer.DBServices;
using System.Xml.Linq;


namespace FlowServer.DBServices
{
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

        public int ChangeTaskStatus(int batchId, int machineId, string status)
        {
            SqlConnection con = null;
            try
            {
                con = connect("igroup16_test1");
                string sqlQuery = "UPDATE MachineBatch SET status=@status WHERE batchId=@batchId and machineId=@machineId";
                Dictionary<string, object> paramDic = new Dictionary<string, object>
                {
                    { "@batchId", batchId },
                    {"@machineId",machineId },
                    { "@status", status }
                };

                using (SqlCommand cmd = CreateCommandWithTextQuery(sqlQuery, con, paramDic))
                {
                    return cmd.ExecuteNonQuery();
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
       

        public int ScheduleTask(int batchId, int machineId,int userId ,DateTime startTimeEst, DateTime endTimeEst)
        {
            SqlConnection con = null;
            try
            {
                CreateTask(batchId, machineId, userId);

                con = connect("igroup16_test1");
                string sqlQuery = "UPDATE MachineBatch SET startTimeEst=@startTimeEst, endTimeEst=@endTimeEst WHERE batchId=@batchId and machineId=@machineId";
                Dictionary<string, object> paramDic = new Dictionary<string, object>
                {
                    { "@batchId", batchId },
                    { "@machineId", machineId },
                    { "@startTimeEst", startTimeEst },
                    { "@endTimeEst", endTimeEst }
                };

                using (SqlCommand cmd = CreateCommandWithTextQuery(sqlQuery, con, paramDic))
                {
                    return cmd.ExecuteNonQuery();
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

        public int CreateTask(int batchId, int machineId, int userId)
        {
            SqlConnection con = null;
            try
            {
                con = connect("igroup16_test1");
                string sqlQuery = "INSERT INTO MachineBatch (batchId, machineId, userId,status) VALUES (@batchId, @machineId, @userId,'Pending')";
                Dictionary<string, object> paramDic = new Dictionary<string, object>
                {
                    { "@batchId", batchId },
                    { "@machineId", machineId },
                    { "@userId", userId }
                };
                using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SPCreateTask", con, paramDic))
                {
                    return cmd.ExecuteNonQuery();
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

        public bool UpdateStartTime(int batchId, int machineId)
        {
            SqlConnection con = null;
            try
            {
                con = connect("YourDatabaseName"); // Replace with your DB name

                string sql = "UPDATE MachineBatch SET startTime = @now WHERE batchId = @batchId AND machineId = @machineId";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@now", DateTime.Now);
                    cmd.Parameters.AddWithValue("@batchId", batchId);
                    cmd.Parameters.AddWithValue("@machineId", machineId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        public bool UpdateEndTime(int batchId, int machineId)
        {
            SqlConnection con = null;
            try
            {
                con = connect("YourDatabaseName"); // Replace with your DB name

                string sql = "UPDATE MachineBatch SET endTime = @now WHERE batchId = @batchId AND machineId = @machineId";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@now", DateTime.Now);
                    cmd.Parameters.AddWithValue("@batchId", batchId);
                    cmd.Parameters.AddWithValue("@machineId", machineId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

    }
}
