using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DAL.Entities;
using Microsoft.IdentityModel.Tokens;

namespace TaskFlowAPI.Security;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SigningCredentials _signingCredentials;
    private readonly TimeSpan _expiresIn;

    public JwtTokenService(string issuer, string audience, string secretKey, TimeSpan expiresIn)
    {
        _issuer = issuer;
        _audience = audience;
        _expiresIn = expiresIn;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTime ExpiresAtUtc) CreateToken(User user)
    {
        var expiresAtUtc = DateTime.UtcNow.Add(_expiresIn);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}

