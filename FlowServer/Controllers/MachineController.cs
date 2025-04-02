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

        [HttpPut("{id}/status")]
        public IActionResult UpdateMachineStatus(int id, [FromBody] int newStatus)
        {
            Machine.ChangeMachineStatus(id, newStatus);
            return Ok();
        }
    }
}