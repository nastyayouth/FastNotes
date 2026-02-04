using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FastNotes.Api.Infrastructure.Config;
using FastNotes.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FastNotes.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwt;

    public AuthController(IOptions<JwtSettings> jwtOptions)
    {
        _jwt = jwtOptions.Value;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("scope", "tasks.read")
        };

        if (request.CanWrite)
        {
            claims.Add(new Claim("scope", "tasks.write"));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.SigningKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.TokenLifetimeMinutes),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            access_token = jwt,
            token_type = "Bearer",
            expires_in = _jwt.TokenLifetimeMinutes * 60
        });
    }
}