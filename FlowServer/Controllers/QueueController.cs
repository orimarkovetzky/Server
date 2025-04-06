using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Queue = FlowServer.Models.Queue;
using Task = FlowServer.Models.Task;

namespace FlowServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        [HttpGet("GetMachineQueue/{machineId}")]
        public IActionResult GetMachineQueue(int machineId)
        {
            try
            {
                List<Task> tasks = Queue.GetMachineQueue(machineId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
