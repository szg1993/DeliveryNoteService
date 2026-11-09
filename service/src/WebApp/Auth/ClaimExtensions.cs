using Common.Core.Exceptions;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace DeliveryNoteService.WebApp.Auth;

public static class ClaimExtensions
{
    public static int GetSubjectIdentifier(this ClaimsPrincipal principal)
        => principal.GetClaimValue<int>(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

    public static TClaimValue GetClaimValue<TClaimValue>(this ClaimsPrincipal principal, string claimName) where TClaimValue : IParsable<TClaimValue>
        => Parse<TClaimValue>(principal.FindFirstValue(claimName));

    public static TClaimValue GetClaimValue<TClaimValue>(this AuthenticationHeaderValue jwt, string claimName) where TClaimValue : IParsable<TClaimValue>
    {
        var value = JwtSecurityTokenConverter
            .Convert(new JsonWebToken(jwt.Parameter))
            .Claims
            .FirstOrDefault(c => c.Type == claimName)?.Value;

        return Parse<TClaimValue>(value);
    }

    private static T Parse<T>(string? claimValue) where T : IParsable<T>
    {
        if (T.TryParse(claimValue, CultureInfo.InvariantCulture, out T? parsedValue) && parsedValue is not null)
            return parsedValue;

        throw new AuthenticationException($"Claim value {claimValue} cannot be parsed to type {typeof(T).Name}");
    }
}