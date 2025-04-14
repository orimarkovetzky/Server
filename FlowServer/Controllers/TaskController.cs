using FlowServer.DBServices;
using FlowServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Task = FlowServer.Models.Task;

namespace FlowServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {

        [HttpPut("{taskId}/{machineId}/{status}")]
        public ActionResult UpdateTaskStatus(int taskId, int machineId, string status)
        {
            int result = Task.UpdateTaskStatus(taskId, machineId, status);

            if (result > 0)
                return Ok($"Task {taskId} status updated to '{status}'");
            else
                return NotFound($"Task with ID {taskId} not found.");
        }

        [HttpGet("GetTasksByBatchId/{batchId}")]
        public IActionResult GetTasksByBatchId(int batchId)
        {
            try
            {
                TaskDBServices dbs = new TaskDBServices();
                List<Task> tasks = dbs.GetTasksByBatchId(batchId);

                if (tasks == null || tasks.Count == 0)
                    return NotFound($"No tasks found for batch {batchId}");

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }
    }

}
