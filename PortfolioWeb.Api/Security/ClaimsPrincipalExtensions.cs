using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace PortfolioWeb.Api.Security;

internal static class ClaimsPrincipalExtensions
{
    public static bool TryGetAuthorId(this ClaimsPrincipal user, out Guid authorId)
    {
        var authorIdValue = user.FindFirst("authorId")?.Value;

        return Guid.TryParse(authorIdValue, out authorId) && authorId != Guid.Empty;
    }

    public static bool TryGetEmail(this ClaimsPrincipal user, out string email)
    {
        email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? user.FindFirst(ClaimTypes.Email)?.Value
            ?? string.Empty;

        return !string.IsNullOrWhiteSpace(email);
    }
}
