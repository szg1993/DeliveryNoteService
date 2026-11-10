using Common.AspNetCore;
using Common.AspNetCore.ErrorHandling;
using Common.AspNetCore.Logging;
using Common.AspNetCore.Swagger;
using Common.Core.DependencyInjection;
using Common.Core.Infrastructure;
using Common.Core.Integration;
using Common.Core.Versioning;
using DeliveryNoteService.Persistence;
using DeliveryNoteService.WebApp.Configuration;
using DeliveryNoteService.WebApp.Correlation;
using DeliveryNoteService.WebApp.Diag;
using DeliveryNoteService.WebApp.HealthProbes;
using DeliveryNoteService.WebApp.RestApi;

string moduleName = "Demo Service";

var builder = WebApplication.CreateBuilder(args);

builder.UseLocalAppsettings();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services
    .AddEndpointsApiExplorer()
    .AddMedusaHttp()
    .AddJsonOptions()
    .AddHealthProbes()
    .AddVersioning(builder.Configuration.GetSection("Versioning"))
    .AddSwaggerGen()
    .AddLogging(moduleName, builder.Configuration.GetSection("Logging"))
    .AddCorrelationId()
    .AddInfrastructure()
    .AddDeliveryNoteServicePersistence(builder.Configuration.GetSection("Persistence"))
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
    .UseMiddleware<RequiredHeaderValidationMiddleware>();

app
    .MapVersionsEndpoint(moduleName)
    .MapRecipientRoutes();

await app.RunAsync(
    initServices: async app =>
    {
        await PersistenceInit.InitDatabaseAsync(app.Services);
    }
);

// required for Minimal API to work with WebApplicationFactory<Program>
public partial class Program
{
    private Program() { }
}