using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlowServer.Models;
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

        [HttpPut("{id}/status")]
        public ActionResult UpdateBatchStatus(int id, [FromBody] string newStatus)
        {
            int result = Batch.UpdateBatchStatus(id, newStatus);

            if (result > 0)
                return Ok($"Batch {id} status updated to '{newStatus}'");
            else
                return NotFound($"Batch with ID {id} not found.");
        }
    }
}


