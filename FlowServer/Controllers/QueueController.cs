using FlowServer.DBServices;
using FlowServer.Models;
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

        [HttpGet("GetBatchesByStatus/{status}")]
        public IActionResult GetBatchesByStatus(string status)
        {
            try
            {
                QueueDBServices dbs = new QueueDBServices();
                List<Batch> batches = dbs.GetBatchesByStatus(status);

                if (batches == null || batches.Count == 0)
                    return NotFound($"No batches found with status '{status}'.");

                return Ok(batches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

    }
    }
