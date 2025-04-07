
using FlowServer.Models;
using Microsoft.AspNetCore.Mvc;
using FlowServer.DBServices;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    [HttpGet("GetAllUsers")]
    public ActionResult GetAllUsers()
    {
        UserDBServices dbs = new UserDBServices();
        List<User> users = dbs.ReadUsers();
        if (users != null && users.Count > 0)
            return Ok(users);
        else
            return NotFound("No users found.");
    }
}