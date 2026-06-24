using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PortfolioWeb.Application.Contract.DTOs;
using PortfolioWeb.Domain.Entities;

namespace PortfolioWeb.Application.Security;

internal static class JwtTokenFactory
{
    public static AuthResponseDTO Create(User user, IConfiguration configuration)
    {
        var issuer = configuration["Authentication:Issuer"] ?? throw new InvalidOperationException("Authentication issuer is not configured.");
        var audience = configuration["Authentication:Audience"] ?? throw new InvalidOperationException("Authentication audience is not configured.");
        var signingKey = configuration["Authentication:SigningKey"] ?? throw new InvalidOperationException("Authentication signing key is not configured.");
        var expirationMinutes = int.TryParse(configuration["Authentication:ExpirationMinutes"], out var parsedMinutes)
            ? parsedMinutes
            : throw new InvalidOperationException("Authentication expiration is not configured.");

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("authorId", user.Author.Id.ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AuthResponseDTO
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }
}
