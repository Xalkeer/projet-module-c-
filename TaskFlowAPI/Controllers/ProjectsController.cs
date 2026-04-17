using Microsoft.AspNetCore.Mvc;
using DAL;
using DAL.Entities;

namespace TaskFlowAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly TaskFlowContext _context;

    public ProjectsController(TaskFlowContext context)
    {
        _context = context;
    }

    // GET: api/projects
    [HttpGet]
    public IActionResult GetProjects()
    {
        var projects = _context.Projects.ToList();
        return Ok(projects);
    }

    // GET: api/projects/{id}
    [HttpGet("{id}")]
    public IActionResult GetProject(int id)
    {
        var project = _context.Projects.Find(id);
        if (project == null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    
    // Post : api/projects/{project}
    [HttpPost]
    public async Task<ActionResult<DAL.Entities.Project>> CreateProject(DAL.Entities.Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }
    
    


}
