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

        [HttpPut("ScheduleTask")]
        public IActionResult UpdateStartTimeEst(int batchId, int machineId, DateTime startTimeEst, DateTime endTimeEst)
        {
            try
            {
                int result = Task.ScheduleTask(batchId, machineId, startTimeEst,endTimeEst);
                if (result > 0)
                {
                    return Ok("Task was scheduled");
                }
                else
                {
                    return NotFound("Task not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("CreateTask")]

        public IActionResult CreateTask(int batchId, int machineId, int userId)
        {
            try
            {
                int result = Task.CreateTask(batchId, machineId, userId);
                if (result > 0)
                {
                    return Ok("Task was created");
                }
                else
                {
                    return NotFound("Task not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }

}
