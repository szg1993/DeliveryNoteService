using Common.Core.Identity;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace Common.AspNetCore.Auth;

public class AspIdentityContext(IHttpContextAccessor httpContextAccessor) : IIdentityContext
{
    public bool IsAuthenticated => httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? AuthToken
    {
        get
        {
            var authHeader = httpContextAccessor?.HttpContext?.Request?.Headers.Authorization.FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            return authHeader["Bearer ".Length..];
        }
    }

    public bool IsSysAdmin
    {
        get
        {
            var isSysAdminClaim = httpContextAccessor?.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type == "isSysAdmin");

            if (string.IsNullOrEmpty(isSysAdminClaim?.Value))
                return false;

            return bool.Parse(isSysAdminClaim.Value);
        }
    }

    public int? UserId => httpContextAccessor?.HttpContext?.User?.GetSubjectIdentifier();

    public string? UserName => httpContextAccessor?.HttpContext?.User?.GetClaimValue<string>(JwtRegisteredClaimNames.Email);

    public string? Audience => httpContextAccessor?.HttpContext?.User?.GetClaimValue<string>(JwtRegisteredClaimNames.Aud);
}