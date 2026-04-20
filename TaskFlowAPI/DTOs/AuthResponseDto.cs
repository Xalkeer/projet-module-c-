namespace TaskFlowAPI.DTOs;

public sealed class AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public UserResponseDto User { get; init; } = new();
}

