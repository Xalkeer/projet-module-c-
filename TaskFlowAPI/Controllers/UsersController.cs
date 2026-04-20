using DAL;
using DAL.Entities;
using DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Security;

namespace TaskFlowAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly TaskFlowContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public UsersController(
        TaskFlowContext context,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
    {
        var users = await _context.Users
            .Select(user => new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            })
            .ToListAsync();

        return Ok(users);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponseDto>> GetUser(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid id",
                Detail = "The user id must be greater than 0.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var user = await _context.Users
            .Where(currentUser => currentUser.Id == id)
            .Select(currentUser => new UserResponseDto
            {
                Id = currentUser.Id,
                Name = currentUser.Name,
                Email = currentUser.Email,
                Role = currentUser.Role
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"No user exists with id {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(user);
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> CreateUser(RegisterUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existingUser = await _context.Users.AnyAsync(user => user.Email == request.Email);
        if (existingUser)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Email already in use",
                Detail = "A user with this email already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Role = request.Role == default ? Role.User : request.Role
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var authResponse = CreateAuthResponse(user);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, authResponse);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _context.Users.SingleOrDefaultAsync(currentUser => currentUser.Email == request.Email);
        if (user == null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Invalid credentials.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Invalid credentials.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        return Ok(CreateAuthResponse(user));
    }

    private AuthResponseDto CreateAuthResponse(User user)
    {
        var (token, expiresAtUtc) = _jwtTokenService.CreateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            }
        };
    }
}