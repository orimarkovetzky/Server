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
            var paramDic = new Dictionary<string, object>
    {
        { "@batchId", batchId },
        { "@machineId", machineId },
        { "@status", status }
    };

            using (SqlConnection con = connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("ChangeTaskStatus", con, paramDic))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public int ScheduleTask(int batchId, int machineId, int userId, DateTime startTimeEst, DateTime endTimeEst)
        {
            // This is assumed to insert a new row or ensure task exists
            CreateTask(batchId, machineId, userId);

            var paramDic = new Dictionary<string, object>
    {
        { "@batchId", batchId },
        { "@machineId", machineId },
        { "@startTimeEst", startTimeEst },
        { "@endTimeEst", endTimeEst }
    };

            using (SqlConnection con = connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("ScheduleTask", con, paramDic))
            {
                return cmd.ExecuteNonQuery();
            }
        }
        public int CreateTask(int batchId, int machineId, int userId)
        {
            var paramDic = new Dictionary<string, object>
    {
        { "@batchId", batchId },
        { "@machineId", machineId },
        { "@userId", userId }
    };

            using (SqlConnection con = connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("SPCreateTask", con, paramDic))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public bool UpdateStartTime(int batchId, int machineId)
        {
            var paramDic = new Dictionary<string, object>
    {
        { "@batchId", batchId },
        { "@machineId", machineId },
        { "@now", DateTime.Now }
    };

            using (SqlConnection con = connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("UpdateStartTime", con, paramDic))
            {
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public bool UpdateEndTime(int batchId, int machineId)
        {
            var paramDic = new Dictionary<string, object>
    {
        { "@batchId", batchId },
        { "@machineId", machineId },
        { "@now", DateTime.Now }
    };

            using (SqlConnection con = connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("UpdateEndTime", con, paramDic))
            {
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

    }
}
