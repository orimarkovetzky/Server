namespace FlowServer.Models
{
    public class BatchView
    {
        public int BatchId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ProductType { get; set; } // 0 for O-Ring, 1 for Fingers
        public string Color { get; set; } 
        public DateTime SupplyDate { get; set; }
        public List<TaskView> Tasks { get; set; } = new List<TaskView>();

        public BatchView(int batchId, int orderId, int productId, string productName, int quantity, string status, int productType, string color, DateTime supplyDate)
        {
            BatchId = batchId;
            OrderId = orderId;
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            Status = status;
            ProductType = productType;
            Color = color;
            SupplyDate = supplyDate;
        }

        public BatchView() { } // default constructor

        public static List<BatchView> GetProductPageData()
        {
            BatchDBServices dbs = new BatchDBServices();
            return dbs.GetProductPageData();
        }

    }
}
