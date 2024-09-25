using LiL.TimeTracking.Resources;
using Microsoft.AspNetCore.Mvc;

namespace Lil.TimeTracking.Controllers;

[Route("")]
[ApiController]
public class HomeController:ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<Resource>>(StatusCodes.Status200OK)]
    public IActionResult Get(){
        var resources = new List<Resource>{
            new Resource("Employees", "/api/Employee"),
            new Resource("Projects", "/api/Project"),
            new Resource("Time Entries", "/api/TimeEntry"),
            new Resource("Project Assignments", "/api/ProjectAssignment")
        };

        return Ok(resources);
    }
}