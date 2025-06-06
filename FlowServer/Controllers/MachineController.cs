﻿using Microsoft.AspNetCore.Mvc;
using FlowServer.Models;
using FlowServer.DBServices;
using System.Security.Cryptography.X509Certificates;
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

        private readonly MachineDBServices dbs = new MachineDBServices(); 

        [HttpGet("machinecards")]
       
        public ActionResult<List<MachineCard>> GetMachineCards()
        {
            try
            {
                var machineCards = dbs.GetMachineCards();
                return Ok(machineCards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while fetching machine cards: " + ex.Message);
            }
        }

        [HttpGet("Machine2tasks-overview")]
        public IActionResult GetMachineTasksOverview()
        {
            try
            {
                var service = new MachineDBServices();
                var result = service.GetMachineTasksOverview();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}