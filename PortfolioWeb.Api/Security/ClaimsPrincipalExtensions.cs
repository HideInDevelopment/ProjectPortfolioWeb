using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace PortfolioWeb.Api.Security;

internal static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal user)
    {
        public bool TryGetAuthorId(out Guid authorId)
        {
            var authorIdValue = user.FindFirst("authorId")?.Value;

            return Guid.TryParse(authorIdValue, out authorId) && authorId != Guid.Empty;
        }

        public bool TryGetEmail(out string email)
        {
            email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                    ?? user.FindFirst(ClaimTypes.Email)?.Value
                    ?? string.Empty;

            return !string.IsNullOrWhiteSpace(email);
        }
    }
}
