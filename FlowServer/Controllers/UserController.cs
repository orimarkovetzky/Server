
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


        [HttpGet("isManager/{userId}")]
        public IActionResult IsManager(int userId)
        {
            try
            {
                User user = new User { Id = userId };
                bool isManager = user.CheckManager();
                return Ok(new { userId, isManager });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error checking manager status", error = ex.Message });
            }
        }
    }
