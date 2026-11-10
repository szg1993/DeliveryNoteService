using Common.AspNetCore.Swagger.Annotations;
using Common.Core.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.ComponentModel;

namespace Common.AspNetCore.Swagger;

public static class SwaggerInit
{
    private const string _bearerScheme = "Bearer";

    public static IApplicationBuilder UseMedusaSwagger(this IApplicationBuilder app, IConfigurationSection settings)
    {
        if (settings.Get<SwaggerSettings>()?.Enabled == true)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    public static IServiceCollection AddSwaggerGen(this IServiceCollection services, string moduleName)
        => AddSwaggerGen(services, moduleName, _ => { });

    public static IServiceCollection AddSwaggerGen(this IServiceCollection services, string moduleName, Action<SwaggerGenOptions> setupAction)
    {
        return services
            .AddSwaggerGen(opt =>
            {
                opt.AddTitle(services, moduleName);
                opt.UseDefaultTag(moduleName);
                opt.UseAllOfToExtendReferenceSchemas(); // "nullable" property as sibling to $ref is ignored and skipped. Use "allOf" instead.
                opt.DocumentFilter<SortPathsDocumentFilter>();
                opt.SchemaFilter<EnumSchemaFilter>();
                opt.SchemaFilter<DtoPropertyDescriptionFilter>();
                opt.CustomSchemaIds(GetSwaggerSchemaId);
                opt.AddAuthorization();
                opt.SupportNonNullableReferenceTypes();
                opt.OperationFilter<OperationIdFilter>();
                opt.OperationFilter<AuthorizationFilter>();
                opt.OperationFilter<HeaderParameterFilter>();
                opt.OperationFilter<ParameterDescriptionFilter>();
                opt.OperationFilter<ResponseDescriptionFilter>();
                opt.OperationFilter<FileResultFilter>();
                opt.OperationFilter<ParameterSchemaOverwriteFilter>();
                setupAction(opt);
            })
            .Configure<SwaggerUIOptions>(options =>
            {
                options.DocExpansion(DocExpansion.None);
            });
    }

    private static void AddAuthorization(this SwaggerGenOptions opt)
    {
        opt.AddSecurityDefinition(_bearerScheme, new OpenApiSecurityScheme()
        {
            Description = "JWT Authorization",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Scheme = _bearerScheme,
            Type = SecuritySchemeType.ApiKey
        });

        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = _bearerScheme
                            }
                        },
                        new List<string>()
                    }
                });
    }

    private static void AddTitle(this SwaggerGenOptions opt, IServiceCollection services, string moduleName)
    {
        var localVersionService = services.BuildServiceProvider().GetRequiredService<ILocalVersionService>();

        opt.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = moduleName,
            Version = localVersionService!.GetVersion().ToString()
        });
    }

    private static void UseDefaultTag(this SwaggerGenOptions opt, string defaultTag)
    {
        opt.TagActionsBy(api =>
        {
            var tags = api.ActionDescriptor.EndpointMetadata
                .OfType<ITagsMetadata>()
                .FirstOrDefault()?.Tags;

            if (tags is { Count: > 0 })
                return [.. tags];

            return [defaultTag];
        });
    }

    private static string GetSwaggerSchemaId(Type t)
    {
        if (TryGetCustomDisplayName(t, out var displayName))
            return displayName!;

        if (t.IsGenericType)
            return GetGenericTypeName(t);

        return t.Name;
    }

    private static bool TryGetCustomDisplayName(Type t, out string? displayName)
    {
        displayName = t.GetCustomAttributes(false).OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName;
        return displayName is not null;
    }

    private static string GetGenericTypeName(Type t)
    {
        var genericTypeName = t.Name;
        // Remove the `1 from the name if it exists (e.g., "Envelope`1")
        var backtickIndex = genericTypeName.IndexOf('`');
        if (backtickIndex > 0)
            genericTypeName = genericTypeName.Remove(backtickIndex);

        return $"{genericTypeName}<{string.Join(", ", t.GetGenericArguments().Select(GetGenericTypeName))}>";
    }
}