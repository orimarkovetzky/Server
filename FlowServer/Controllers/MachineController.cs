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

        [HttpPut("{id}/{status}")]
        public ActionResult UpdateMachineStatus(int id, int status)
        {
           int result = Machine.UpdateMachineStatus(id, status);

            if (result > 0)
                return Ok($"Machine {id} status updated to '{status}'");
            else
                return NotFound($"Machine with ID {id} not found.");
        }
    }
}