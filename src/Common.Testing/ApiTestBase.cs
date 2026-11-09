using Common.Core.HealthProbes;
using Common.Core.Logging;
using Common.Testing.Database;
using Common.Testing.Infrastructure;
using Common.Testing.Logging;
using Common.Testing.TestSuite;
using Common.Testing.Versioning;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Common.Testing;

public abstract class ApiTestBase<TWebAppEntryPoint, TApi>(string? testId = null) :
    IScenario,
    IAsyncLifetime
    where TWebAppEntryPoint : class
    where TApi : Api
{
    protected WebApplicationFactory<TWebAppEntryPoint> WebAppFactory { get; private set; } = default!;
    protected readonly string TestId = testId ?? Guid.NewGuid().ToString();

    #region Injected Properties

    /* XUnit Property injection needs the following
     * - package reference to XUnit.DependencyInjection -> this hooks up Startup class, transitive reference is enough
     * - Startup class with the proper method(s)
     *      in the actual Test Project... Common.Testing is not enough. This will have assembly-level hooks.
     * - PropertyInjected types must be registered in Startup into the test DI
     *   - IAsyncLifetime is NO LONGER respected on the fixtures
     *   - initialization and teardown must be done explicitly
     * - Properties must be 'required' (otherwise they won't be set)
     * - do NOT mix with IClassFixture<TProperty>
     *     - it will instantiate another instance but won't set it via property
     */
    public required ITestOutputHelper TestOutput { get; init; }
    public required DatabaseFixture DatabaseFixture { get; init; }

    #endregion Injected Properties

    /// <summary>
    /// Syntactic sugar to access Scenario extension methods so that the test code reads better: "await Given.Pod()";
    /// </summary>
    public Scenario<TApi> Given { get; private set; } = default!;
    public TApi Api { get; private set; } = default!;
    Api IScenario.Api => Api;

    public IServiceProvider Services => WebAppFactory.Services;
    protected virtual IDatabaseFixture TestDatabase => DatabaseFixture;
    protected NullSortingStyle NullSortingStyle => TestDatabase.NullSortingStyle;
    protected TestLogStore LogStore => Services.GetRequiredService<TestLogStore>();

    #region IAsyncLifetime members

    public virtual async Task InitializeAsync()
    {
        WriteLog("Initializing test. TestID: " + TestId);
        var testDbConnectionString = await TestDatabase.CreateTestDbAsync(TestId);

        WebAppFactory = new MedusaWebAppFactory<TWebAppEntryPoint>()
            .ConfigureHostConfiguration(configBuilder =>
            {
                configBuilder.AddConnectionString(testDbConnectionString);
                configBuilder.DisableStartupJobs(true);
                ConfigureTestHostConfiguration(configBuilder);
            })
            .WithWebHostBuilder(builder =>
            {
                builder
                    .ConfigureAppConfiguration((ctx, builder) =>
                    {
                        ConfigureTestAppConfiguration(ctx, builder);
                    })
                    .ConfigureTestServices(serviceCollection =>
                    {
                        ConfigureServices(serviceCollection);
                        TestDatabase.ConfigureTestServices(TestId, serviceCollection);
                    });
            });

        Api = CreateApiWrapper(WebAppFactory.CreateDefaultClient());
        Given = new Scenario<TApi>(Api, WebAppFactory.Services);
        await WaitForWebAppReady();
        WriteLog("ApiTestBase initialization DONE. WebApp ready.");
    }

    public virtual async Task DisposeAsync()
    {
        WriteLog("ApiTestBase DisposeAsync started.");
        if (WebAppFactory is not null)
        {
            await WebAppFactory.DisposeAsync();
            Api.Client?.Dispose();
        }

        await TestDatabase.ClearTestDbAsync(TestId);
        WriteLog("ApiTestBase DisposeAsync finished.");
    }

    #endregion

    protected abstract TApi CreateApiWrapper(HttpClient client);

    /// <summary>
    /// Override this method (and call base implementation) to overwrite any production services
    /// or common test services.
    /// </summary>
    /// <param name="services"></param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services
            .Configure<QuartzOptions>(options =>
            {
                options[StdSchedulerFactory.PropertySchedulerInstanceName] = $"testScheduler_{TestId}";
                options[StdSchedulerFactory.PropertySchedulerInstanceId] = $"testSchedulerInstance_{TestId}";
                options["quartz.threadPool.maxConcurrency"] = "1000";
            })
            .AddTestMedusaLogging(TestId, TestOutput)
            .AddTestVersionService()
            .AddTestTimeService()
            .AddTestHttpClient()
            ;
    }

    /// <summary>
    /// Override this method to overwrite any appsettings/config values before web app startup.
    /// </summary>
    protected virtual void ConfigureTestHostConfiguration(IConfigurationBuilder configBuilder) { }

    /// <summary>
    /// Override this method to overwrite any config after the app is built but not yet started.
    /// </summary>
    protected virtual void ConfigureTestAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder configBuilder) { }

    protected static QueryString QS() => QueryString.Empty;

    protected static QueryString QS(string key, string value) => QueryString.Create(key, value);

    protected IDeliveryNoteServiceLogger GetLogger() => Services.GetRequiredService<AppLogger>();

    protected void WriteLog(string message)
    {
        TestOutput.WriteLine($"[{DateTime.Now:HH:mm:ss.fff} TEST] {message}");
    }

    private async Task WaitForWebAppReady()
    {
        var rs = this.Services.GetService<IReadinessService>();
        if (rs is null)
            return;

        var success = await rs.WaitForInitializationResultAsync();
        if (!success)
            throw new XunitException("Failed to initialize web app.");
    }
}