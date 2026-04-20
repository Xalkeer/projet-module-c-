using Microsoft.AspNetCore.Mvc;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

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

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Project>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        var projects = await _context.Projects.ToListAsync();
        return Ok(projects);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid id",
                Detail = "The project id must be greater than 0.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Project not found",
                Detail = $"No project exists with id {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(project);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Project), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

}
