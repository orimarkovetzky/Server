using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlowServer.Models;
using FlowServer.DBServices;
namespace FlowServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatchController : ControllerBase
    {
        [HttpGet("{id}")]
        public ActionResult<Batch> Get(int id)
        {
            var batch = Batch.FindBatch(id);

            if (batch == null)
                return NotFound($"Batch with ID {id} not found.");

            return Ok(batch);
        }

        [HttpGet]
        [Route("GetProductPageData")]
        public ActionResult<List<BatchView>>GetProductPageData()
        {
            try
            {
                List<BatchView> batches = BatchView.GetProductPageData();
                if (batches == null || batches.Count == 0)
                    return NotFound("No batches found.");
                return Ok(batches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

        [HttpPut("{id}/{status}")]
        public ActionResult UpdateBatchStatus(int id, string status)
        {
            int result = Batch.UpdateBatchStatus(id, status);

            if (result > 0)
                return Ok($"Batch {id} status updated to '{status}'");
            else
                return NotFound($"Batch with ID {id} not found.");
        }

        [HttpGet("GetTasksByBatchId/{batchId}")]
        public IActionResult GetTasksByBatchId(int batchId)
        {
            try
            {
                BatchDBServices dbs = new BatchDBServices();
                List<dynamic> tasks = dbs.GetTasksByBatchId(batchId);

                if (tasks == null || tasks.Count == 0)
                    return NotFound($"No tasks found for batch {batchId}");

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

        [HttpPost("AssignBatchToQueues/{batchId}/{userId}")]
        public IActionResult AssignBatchToQueues(int batchId, int userId)
        {
            try
            {
                BatchDBServices batchService = new BatchDBServices();
                Batch batch = batchService.FindBatch(batchId); // <-- convert batchId into Batch object

                if (batch == null)
                {
                    return NotFound($"Batch with ID {batchId} not found");
                }

                TaskAssigner.AssignBatchToQueues(batch,userId); // <-- Pass the Batch, not just ID

                return Ok($"Batch {batchId} successfully assigned to machines");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}


