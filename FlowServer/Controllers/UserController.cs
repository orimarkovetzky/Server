
using FlowServer.Models;
using Microsoft.AspNetCore.Mvc;
using FlowServer.DBServices;
using user = FlowServer.Models.User;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    [HttpGet("GetAllUsers")]
    public ActionResult GetAllUsers()
    {
        UserDBServices dbs = new UserDBServices();
        List<user> users = dbs.ReadUsers();
        if (users != null && users.Count > 0)
            return Ok(users);
        else
            return NotFound("No users found.");
    }


  
    [HttpPost("CreateUser")]
    public IActionResult CreateUser([FromBody] user newUser)
    {
        try
        {
            user.CreateUser(newUser);
            return Ok("User created successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error: " + ex.Message);
        }
    }
}