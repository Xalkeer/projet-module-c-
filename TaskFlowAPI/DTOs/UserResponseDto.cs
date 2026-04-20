using DAL.Enums;

namespace TaskFlowAPI.DTOs;

public sealed class UserResponseDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Role Role { get; init; }
}

