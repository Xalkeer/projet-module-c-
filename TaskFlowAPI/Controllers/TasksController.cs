using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskEntity = DAL.Entities.Task;

namespace TaskFlowAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly TaskFlowContext _context;

    public TasksController(TaskFlowContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<TaskEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskEntity>>> GetTasks()
    {
        var tasks = await _context.Tasks.ToListAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(TaskEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskEntity>> GetTask(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid id",
                Detail = "The task id must be greater than 0.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found",
                Detail = $"No task exists with id {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(task);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(TaskEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskEntity>> CreateTask(TaskEntity task)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTask(int id, TaskEntity task)
    {
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid id",
                Detail = "The task id must be greater than 0.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (id != task.Id)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Payload id mismatch",
                Detail = "The route id must match the task id in the payload.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        _context.Entry(task).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Tasks.AnyAsync(t => t.Id == id))
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Task not found",
                    Detail = $"No task exists with id {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Conflict(new ProblemDetails
            {
                Title = "Concurrency conflict",
                Detail = "The task was modified by another request. Refresh and retry.",
                Status = StatusCodes.Status409Conflict
            });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid id",
                Detail = "The task id must be greater than 0.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found",
                Detail = $"No task exists with id {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}