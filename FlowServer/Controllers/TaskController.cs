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
        public ActionResult UpdateTaskStatus(int taskId,int machineId, string status)
        {
            int result = Task.UpdateTaskStatus(taskId,machineId, status);

            if (result > 0)
                return Ok($"Task {taskId} status updated to '{status}'");
            else
                return NotFound($"Task with ID {taskId} not found.");
        }

    }
}
