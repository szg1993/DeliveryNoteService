using Medusa.Enap.Common.AspNetCore.Auth;
using Microsoft.Extensions.Configuration;

namespace Medusa.Enap.Common.Testing.Auth;

public static class TestAuth
{
    public static void ConfigureTestAuth(this IConfigurationBuilder configBuilder)
    {
        // used by the JwtBearer middleware to verify token signature
        configBuilder.Add("Auth:" + nameof(MedusaAuthSettings.TokenSigningPublicKey), MockSigningKeys.Public);
    }
}
