using System.Security.Claims;

namespace PortfolioWeb.Api.Security;

internal static class ClaimsPrincipalExtensions
{
    public static bool TryGetAuthorId(this ClaimsPrincipal user, out Guid authorId)
    {
        var authorIdValue = user.FindFirst("authorId")?.Value;

        return Guid.TryParse(authorIdValue, out authorId) && authorId != Guid.Empty;
    }
}
