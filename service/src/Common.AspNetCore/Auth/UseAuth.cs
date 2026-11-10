using Common.Core;
using Common.Core.Exceptions;
using Common.Core.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Common.AspNetCore.Auth;

public static class UseAuth
{
    /// <summary>
    /// Adds JwtBearer authentication, verifying token signature with the public signing key
    /// of IAM service
    /// </summary>
    public static IServiceCollection AddMedusaAuth(this IServiceCollection services, IConfiguration config)
    {
        return services
            .AddHttpContextAccessor()
            .AddScoped<IIdentityContext, AspIdentityContext>()
            .AddAuthorization(ConfigureAuthorization)
            .AddAuthentication()
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                opt =>
                {
                    var authSettings = config.Get<MedusaAuthSettings>()!;
                    opt.TokenValidationParameters.IssuerSigningKey = GetSecurityKey(authSettings.TokenSigningPublicKey);
                    opt.TokenValidationParameters.ValidateIssuer = false;
                    opt.TokenValidationParameters.ValidateAudience = false;
                    opt.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
                    opt.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            if (context.AuthenticateFailure is SecurityTokenExpiredException)
                                throw new AuthenticationException(CommonErrorCodes.AuthTokenExpired);

                            throw new AuthenticationException(CommonErrorCodes.AuthenticationRequired);
                        },
                        OnForbidden = context =>
                        {
                            throw new AuthorizationException();
                        }
                    };
                    opt.MapInboundClaims = false;
                }
            )
            .Services
            ;
    }

    public static IApplicationBuilder UseMedusaAuth(this IApplicationBuilder app)
    {
        return app
            .UseAuthentication()
            .UseAuthorization()
            ;
    }

    private static void ConfigureAuthorization(AuthorizationOptions opt)
    {
        opt.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireAssertion(context => context.User.HasClaim("scope", JwtScopes.Normal))
            .Build();

        opt.AddPolicy(Policies.MfaSetup, policy =>
        {
            policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("scope", JwtScopes.Normal, JwtScopes.MfaSetup);
        });
    }

    private static RsaSecurityKey GetSecurityKey(string signingPublicKey)
    {
        var publicKeyBytes = Convert.FromBase64String(signingPublicKey);
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(publicKeyBytes, out _);
        return new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: false));
    }
}