using System.Data.SqlClient;
using FlowServer.DBServices;
namespace FlowServer.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime SupplyDate { get; set; }
        public string CustomerName { get; set; }

        public List<Batch> Batches { get; set; }
        public Order() { }

        public Order(int orderId, int customerId, DateTime orderDate, DateTime supplyDate, string customerName)
        {
            OrderId = orderId;
            CustomerId = customerId;
            OrderDate = orderDate;
            SupplyDate = supplyDate;
            CustomerName = customerName;
        }

        public static List<Order> GetAllOrders()
        {
            OrderDBServices dbs = new OrderDBServices();
            return dbs.GetAllOrders();
        }

        public int InsertOrder(int customerId, DateTime orderDate, DateTime supplyDate, SqlConnection con, SqlTransaction tx)
        { 
            OrderDBServices dbs = new OrderDBServices();
            return dbs.InsertOrder(customerId, orderDate, supplyDate,con, tx);
        
        }

    }
}
