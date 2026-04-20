using System.ComponentModel.DataAnnotations;
using DAL.Enums;

namespace TaskFlowAPI.DTOs;

public sealed class RegisterUserRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public Role Role { get; set; } = Role.User;
}

