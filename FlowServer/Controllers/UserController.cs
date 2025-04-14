
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


    [HttpPost]
    public IActionResult Post(string name, bool isManager, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Name or Password missing.");

        var userRequest = new User(name, isManager, password);

        UserDBServices db = new UserDBServices();
        int newId = db.CreateUser(userRequest);
        userRequest.Id = newId;

        return Ok(userRequest);
    }
}