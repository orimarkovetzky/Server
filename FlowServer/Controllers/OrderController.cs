using Microsoft.AspNetCore.Mvc;
using FlowServer.DBServices;
using FlowServer.Models;

namespace FlowServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDBServices _service;

        public OrdersController()
        {
            _service = new OrderDBServices();
        }

        // GET api/orders
        [HttpGet]
        public ActionResult<List<Order>> GetAllOrders()
        {
            var orders = _service.GetAllOrders();

            if (orders == null || !orders.Any())
                return NotFound("No orders found.");

            return Ok(orders);
        }
    }
}
