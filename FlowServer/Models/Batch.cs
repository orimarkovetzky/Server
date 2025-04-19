

namespace FlowServer.Models
{
    public class Batch
    {
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

        public int BatchId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ProductType { get; set; } // 0 for O-Ring, 1 for Fingers

    }

}

 