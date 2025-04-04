using Microsoft.AspNetCore.Mvc;
using FlowServer.Models;
namespace FlowServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineController : Controller
    {
        [HttpGet]
        public IEnumerable<Machine> Get()
        {
            return Machine.ReadMachines();
        }

        [HttpGet("{machineId}")]
        public ActionResult<Machine> Get(int machineId)
        {
            var machine = Machine.FindMachine(machineId);

            if (machine == null)
                return NotFound($"Batch with ID {machineId} not found.");

            return Ok(machine);
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateMachineStatus(int id, [FromBody] int newStatus)
        {
            Machine.ChangeMachineStatus(id, newStatus);
            return Ok();
        }
    }
}