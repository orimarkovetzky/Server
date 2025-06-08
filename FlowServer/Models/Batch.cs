

using System.Data.SqlClient;
using FlowServer.DBServices;

namespace FlowServer.Models
{
    public class Batch
    {

        public int BatchId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ProductType { get; set; } // 0 for O-Ring, 1 for Fingers
        
        public Batch(int batchId, int orderId, int productId, int quantity, string status, int productType) //Full constructor 
        {
            BatchId = batchId;
            OrderId = orderId;
            ProductId = productId;
            Quantity = quantity;
            Status = status;
            ProductType = productType; 

        }
        public Batch() { } // default constructor 
        public static Batch FindBatch(int batchid)
        {
            BatchDBServices dbs = new BatchDBServices();
            return dbs.FindBatch(batchid);
        }
        public static int UpdateBatchStatus(int batchId, string newStatus)
        {
            BatchDBServices dbs = new BatchDBServices();
            return dbs.UpdateBatchStatus(batchId, newStatus);
        }

        public int GetProcessTimeMinutes(int productId, int machineType)
        {
            BatchDBServices dbs = new BatchDBServices();
            return dbs.GetProcessTimeMinutes(productId, machineType);
        }
        public void InsertBatch(int orderId, int productId, int quantity, SqlConnection con, SqlTransaction tx)
        {
            BatchDBServices dbs = new BatchDBServices();
            dbs.InsertBatch(orderId, productId, quantity,con, tx);
           
        }

        public static int GetBatchQueueCount()
        {
            BatchDBServices dbs = new BatchDBServices();
            return dbs.GetBatchQueueCount();
        }

        public static int GetDelayedBatchCount()
        {
            BatchDBServices dbs = new BatchDBServices();
            return dbs.GetDelayedBatchCount();
        }
    }

}

 