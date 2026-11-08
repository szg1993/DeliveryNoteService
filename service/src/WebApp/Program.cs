using Medusa.Enap.Common.AspNetCore;
using Medusa.Enap.Common.AspNetCore.Correlation;
using Medusa.Enap.Common.AspNetCore.Diag;
using Medusa.Enap.Common.AspNetCore.ErrorHandling;
using Medusa.Enap.Common.AspNetCore.HealthProbes;
using Medusa.Enap.Common.AspNetCore.Logging;
using Medusa.Enap.Common.AspNetCore.Redoc;
using Medusa.Enap.Common.AspNetCore.Swagger;
using Medusa.Enap.Common.Core.DependencyInjection;
using Medusa.Enap.Common.Core.Infrastructure;
using Medusa.Enap.Common.Core.Integration;
using Medusa.Enap.Common.Core.UseCases;
using Medusa.Enap.Common.Core.Versioning;
using Medusa.Enap.Common.Infrastructure.Scheduling;
using Medusa.Enap.Common.MessageQueue.RabbitMq;
using Medusa.Enap.DemoService.Application;
using Medusa.Enap.DemoService.Persistence;
using Medusa.Enap.DemoService.WebApp.Configuration;
using Medusa.Enap.DemoService.WebApp.RestApi;

string moduleName = "Demo Service";

var builder = WebApplication.CreateBuilder(args);

builder.UseLocalAppsettings();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services
    .AddEndpointsApiExplorer()
    .AddMedusaHttp()
    .AddMedusaJsonOptions()
    .AddMedusaHealthProbes()
    .AddMedusaVersioning(builder.Configuration.GetSection("Versioning"))
    .AddMedusaSwaggerGen(moduleName)
    .AddMedusaLogging(moduleName, builder.Configuration.GetSection("Logging"))
    .AddMedusaCorrelationId()
    .AddDemoServiceMessageQueue(builder.Configuration.GetSection("Messaging"))
    .AddMedusaInfrastructure()
    .AddMedusaUseCases()
    .AddDemoServiceFeatures(builder.Configuration.GetSection("Features"))
    .AddDemoServicePersistence(builder.Configuration.GetSection("Persistence"))
    .AddDemoServiceScheduling(builder.Configuration)
    .AddDemoServiceApplication()
    .AddMedusaAutoRegisteredServices()
    ;

var app = builder
    .Build()
    .LogLifetimeEvents()
    .UseMedusaHealthProbes()
    .UseMedusaMetrics();

// Configure the HTTP request pipeline.
app
    .UseMedusaCorrelationId()
    .UseMedusaErrorHandling()
    .UseMedusaDiag(app.Configuration.GetSection("Diag"))
    .UseMedusaSwagger(app.Configuration.GetSection("Swagger"))
    .UseMedusaRedoc(app.Configuration.GetSection("Redoc"))
    .UseMiddleware<RequiredHeaderValidationMiddleware>();

app
    .MapVersionsEndpoint(moduleName)
    .MapDemoRoutes()
    .MapRecipientRoutes();

await app.RunAsync(
    initServices: async app =>
    {
        await Task.WhenAll(
            PersistenceInit.InitDatabaseAsync(app.Services),
            RabbitMqClient.InitAsync(app.Services)
        );
        await SchedulingInit.StartSchedulers(app.Services);
    },
    cleanup: async app =>
    {
        await SchedulingInit.StopSchedulers(app.Services);
    }
);

// required for Minimal API to work with WebApplicationFactory<Program>
public partial class Program
{
    private Program() { }
}