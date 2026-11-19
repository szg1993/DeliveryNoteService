using DeliveryNoteService.WebApp.Auth;
using Microsoft.Extensions.Configuration;

namespace ApiTests.Auth;

public static class TestAuth
{
    public static void ConfigureTestAuth(this IConfigurationBuilder configBuilder)
    {
        // used by the JwtBearer middleware to verify token signature
        configBuilder.Add("Auth:" + nameof(AuthSettings.TokenSigningPublicKey), MockSigningKeys.Public);
    }
}
