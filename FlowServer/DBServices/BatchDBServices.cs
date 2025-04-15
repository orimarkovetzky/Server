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


public List<dynamic> GetTasksByBatchId(int batchId)
{
    SqlConnection con = null;
    try
    {
        con = connect("igroup16_test1");

        Dictionary<string, object> paramDic = new Dictionary<string, object>
        {
            { "@BatchId", batchId }
        };

        using (SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetTasksByBatchId", con, paramDic))
        {
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                var tasks = new List<dynamic>();

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
            con.Close();
    }
}

public Batch FindBatch(int BatchId)
    {
        SqlConnection con = null;
        try
        {
            con = connect("igroup16_test1");

            var paramDic = new Dictionary<string, object>
        {
            { "@BatchId", BatchId }
        };

            SqlCommand cmd = CreateCommandWithStoredProcedureGeneral("GetBatchById", con, paramDic);
            SqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                return new Batch()
                {
                    BatchId = Convert.ToInt32(rdr["batchID"]),
                    OrderId = Convert.ToInt32(rdr["orderID"]),
                    ProductId = Convert.ToInt32(rdr["productID"]),
                    Quantity = Convert.ToInt32(rdr["quantity"]),
                    Status = rdr["status"].ToString()
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

    public int UpdateBatchStatus(int batchId, string newStatus)
    {
        SqlConnection con = null;
        try
        {
            con = connect("igroup16_test1");
            string updateStr = "UPDATE Batches SET status = @status WHERE batchID = @batchId";

            Dictionary<string, object> paramDic = new Dictionary<string, object>
            {
                { "@status", newStatus },
                { "@batchId", batchId }
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

    public BatchDBServices()
    {
    }
}
