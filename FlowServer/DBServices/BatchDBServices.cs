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
using FlowServer.DBServices;
using Task = FlowServer.Models.Task;
using System.Dynamic;


public class BatchDBServices
{
    public BatchDBServices() //defualt constructor
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


    public List<dynamic> GetTasksByBatchId(int batchId)
    {
        var tasks = new List<dynamic>();

        using (SqlConnection con = connect("igroup16_test1"))
        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetTasksByBatchId", con, new Dictionary<string, object> { { "@BatchId", batchId } }))
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                dynamic task = new ExpandoObject();
                task.BatchId = reader.GetInt32(reader.GetOrdinal("batchId"));
                task.MachineId = reader.GetInt32(reader.GetOrdinal("machineId"));
                task.StartTimeEst = reader.GetDateTime(reader.GetOrdinal("startTimeEst"));
                task.EndTimeEst = reader.GetDateTime(reader.GetOrdinal("endTimeEst"));
                task.Status = reader.GetString(reader.GetOrdinal("status"));
                tasks.Add(task);
            }
        }

        return tasks;
    }


    public Batch FindBatch(int batchId)
    {
        using (SqlConnection con = connect("igroup16_test1"))
        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetBatchById", con, new Dictionary<string, object> { { "@BatchId", batchId } }))
        using (SqlDataReader rdr = cmd.ExecuteReader())
        {
            if (rdr.Read())
            {
                return new Batch()
                {
                    BatchId = Convert.ToInt32(rdr["batchID"]),
                    OrderId = Convert.ToInt32(rdr["orderID"]),
                    ProductId = Convert.ToInt32(rdr["productID"]),
                    Quantity = Convert.ToInt32(rdr["quantity"]),
                    Status = rdr["status"].ToString(),
                    ProductType = Convert.ToInt32(rdr["type"])
                };
            }

            return null;
        }
    }

    public int UpdateBatchStatus(int batchId, string newStatus)
    {
        var paramDic = new Dictionary<string, object>
    {
        { "@batchId", batchId },
        { "@status", newStatus }
    };

        using (SqlConnection con = connect("igroup16_test1"))
        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("UpdateBatchStatus", con, paramDic))
        {
            return cmd.ExecuteNonQuery(); // number of rows affected
        }
    }

    public void InsertBatch(Batch batch)
    {
        var paramDic = new Dictionary<string, object>
    {
        { "@orderID", batch.OrderId },
        { "@productID", batch.ProductId },
        { "@quantity", batch.Quantity },
        { "@status", "Pending" }
    };

        using (SqlConnection con = connect("DefaultConnection"))
        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("InsertBatchToOrder", con, paramDic))
        {
            cmd.ExecuteNonQuery();
        }
    }
}
