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

        [HttpPut("{id}/{status}")]
        public ActionResult UpdateBatchStatus(int id, string status)
        {
            int result = Batch.UpdateBatchStatus(id, status);

            if (result > 0)
                return Ok($"Batch {id} status updated to '{status}'");
            else
                return NotFound($"Batch with ID {id} not found.");
        }
    }
}


