namespace FlowServer.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime SupplyDate { get; set; }

        public List<Batch> Batches { get; set; }
        public Order() { }

        public Order(int orderId, int customerId, DateTime orderDate, DateTime supplyDate)
        {
            OrderId = orderId;
            CustomerId = customerId;
            OrderDate = orderDate;
            SupplyDate = supplyDate;
        }
    }
}
