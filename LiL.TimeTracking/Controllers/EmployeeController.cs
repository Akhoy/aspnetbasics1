using LiL.TimeTracking.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiL.TimeTracking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //since authorize is in controller level now, all operations require authentication. not only do you need to authenticate the user but you also need to apply this particular policy made up of requirements to validate the user.
    [Authorize(policy:"EmailDomain")]
    public class EmployeeController : ControllerBase
    {
        private TimeTrackingDbContext ctx;
        public EmployeeController(TimeTrackingDbContext context)
        {
            ctx = context;
        }
        // GET: api/<EmployeeController>
        [HttpGet]
        //metadata i can use to let clients know what might come back as a response
        [ProducesResponseType<IEnumerable<Resources.Employee>>(StatusCodes.Status200OK)]
        // This means that I can return a number of different types from this operation. It gives me some flexibility. In our case, we're going to return a list of those employees, but in some cases, we might be returning different status codes like a 404 to indicate something wasn't found. We might want to return error objects versus the result type. And this gives us that flexibility.
        public async Task<IActionResult> Get()
        {
            //TODO: add paging support
            // mapping db model to resource
            var response = ctx.Employees.ProjectToType<Resources.Employee>().AsEnumerable();

            var lEmployees = new List<Resources.LinkedResource<Resources.Employee>>();
            foreach (var e in response)
            {
                var lEmployee = new Resources.LinkedResource<Resources.Employee>(e);
                lEmployee.Links.Add(new Resources.Resource("Projects", $"/api/Employee/{e.Id}/Projects"));
                lEmployees.Add(lEmployee);
            }
            return Ok(lEmployees);
        }

        // GET api/<EmployeeController>/5
        [HttpGet("{id}")]     
        [ProducesResponseType<Resources.Employee>(StatusCodes.Status200OK)]   
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            var dbEmployee = await ctx.Employees.FindAsync(id);
            if(dbEmployee == null){
                return NotFound();
            }
            var response = dbEmployee.Adapt<Resources.Employee>();

            var lEmployee = new Resources.LinkedResource<Resources.Employee>(response);
            lEmployee.Links.Add(new Resources.Resource("Projects", $"/api/Employee/{response.Id}/Projects"));

            return Ok(lEmployee);
        }

        [HttpGet("{id}/Projects")]     
        [ProducesResponseType<IEnumerable<Resources.Project>>(StatusCodes.Status200OK)]   
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProjects(int id)
        {
            var dbEmployee = await ctx.Employees.FindAsync(id);
            if(dbEmployee == null){
                return NotFound();
            }
            else{
                //telling entity framework - take the employee, i want you to load collection related to that i.e. projects
                await ctx.Entry(dbEmployee).Collection(e => e.Projects).LoadAsync();
                var projects = new List<Resources.Project>();
                foreach (var p in dbEmployee.Projects)
                {
                    var rProject = p.Adapt<Resources.Project>();
                    projects.Add(rProject);
                }
            
                return Ok(projects);
            }
        }        

        // POST api/<EmployeeController>
        [HttpPost]
        [ProducesResponseType<Resources.Employee>(StatusCodes.Status201Created)]   
        [ProducesResponseType<ObjectResult>(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]    
        public async Task<IActionResult> Post([FromBody] Resources.Employee value)
        {
            if(!ModelState.IsValid){
                return Problem("Invalid employee request", statusCode: StatusCodes.Status400BadRequest);
            }
            try{
                var dbEmployee = value.Adapt<Models.Employee>();
                await ctx.Employees.AddAsync(dbEmployee);
                await ctx.SaveChangesAsync();
                var response = dbEmployee.Adapt<Resources.Employee>();
                return CreatedAtAction(nameof(Get), new {id=response.Id}, response);
            }
            catch(Exception ex){
                //TODO: Log
                return Problem("Problem persisisting employee resource", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        // PUT api/<EmployeeController>/5
        [HttpPut("{id}")]
        [ProducesResponseType<ObjectResult>(StatusCodes.Status404NotFound)] 
        [ProducesResponseType<Resources.Employee>(StatusCodes.Status201Created)]   
        [ProducesResponseType<ObjectResult>(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> Put(int id, [FromBody] Resources.Employee value)
        {
            if(!ModelState.IsValid){
                return Problem("Invalid employee request", statusCode: StatusCodes.Status400BadRequest);
            }
            try{
                var dbEmployee = value.Adapt<Models.Employee>();
                ctx.Entry<Models.Employee>(dbEmployee).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await ctx.SaveChangesAsync();
                
                return NoContent();
            }
            catch(DbUpdateConcurrencyException ex){
                //TODO: Log
                return Problem("Problem persisisting employee resource", statusCode: StatusCodes.Status500InternalServerError);
            }
            catch(Exception ex){
                //TODO: Log
                var dbEmployee = ctx.Employees.Find(id);
                if(dbEmployee == null){
                    return NotFound();
                }
                else{
                    return Problem("Problem persisisting employee resource", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        }

        // PATCH api/<EmployeeController>/5
        [HttpPatch("{id}")]
        [ProducesResponseType<ObjectResult>(StatusCodes.Status404NotFound)] 
        [ProducesResponseType<Resources.Employee>(StatusCodes.Status204NoContent)]   
        [ProducesResponseType<ObjectResult>(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<Resources.Employee> value)
        {
            if(!ModelState.IsValid){
                return Problem("Invalid employee request", statusCode: StatusCodes.Status400BadRequest);
            }
            try{
                var dbEmployee = await ctx.Employees.FindAsync(id);
                if(dbEmployee == null){
                    return NotFound();
                }

                var employee = dbEmployee.Adapt<Resources.Employee>();

                value.ApplyTo(employee, ModelState);

                var patchedEmployee = employee.Adapt<Models.Employee>();
                ctx.Entry<Models.Employee>(dbEmployee).CurrentValues.SetValues(patchedEmployee);
                await ctx.SaveChangesAsync();
                
                return NoContent();
            }
            
            catch(Exception ex){
                //TODO: Log
                var dbEmployee = ctx.Employees.Find(id);
                if(dbEmployee == null){
                    return NotFound();
                }
                else{
                    return Problem("Problem persisisting employee resource", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        }

        // DELETE api/<EmployeeController>/5
        [HttpDelete("{id}")]
        [ProducesResponseType<ObjectResult>(StatusCodes.Status404NotFound)] 
        [ProducesResponseType<Resources.Employee>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> Delete(int id)
        {
            try{
                var dbEmployee = await ctx.Employees.FindAsync(id);
                if(dbEmployee == null){
                    return NotFound();
                }
                ctx.Employees.Remove(dbEmployee);
                await ctx.SaveChangesAsync();
                return NoContent();
            }
            catch(Exception ex){
                return Problem("Problem deleting employee resource", statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
