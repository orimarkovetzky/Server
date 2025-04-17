using FlowServer.DBServices;
using FlowServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        [HttpPost("AddOrder")]
        public IActionResult AddOrder([FromBody] Order order)
        {
            try
            {
                OrderDBServices orderDB = new OrderDBServices();
                BatchDBServices batchDB = new BatchDBServices();

                int newOrderId = orderDB.InsertOrder(order);

                foreach (Batch batch in order.Batches)
                {
                    batch.OrderId = newOrderId;
                    batchDB.InsertBatch(batch);
                }

                return Ok(new { OrderID = newOrderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inserting order and batches: {ex.Message}");
            }
        }
    }
}
