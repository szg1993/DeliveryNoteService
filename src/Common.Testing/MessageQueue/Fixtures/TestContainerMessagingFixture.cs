using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;
using Testcontainers.RabbitMq;

namespace Medusa.Enap.Common.Testing.MessageQueue.Fixtures;

internal class TestContainerMessagingFixture(CreateTopologyConfig createTopologyConfig) : IMessagingFixture
{
    private const int _numberOfInstances = 1;
    private readonly List<RabbitMqContainer> _instances = [];
    private readonly SemaphoreSlim _mutex = new(1);
    private const int _defaultBrokerPort = 5672;
    private const int _defaultManagementPort = 15672;
    private readonly List<int> _portsAssigned = [];

    public Task InitializeAsync() => CreateTestContainers();

    public Task DisposeAsync() => Task.WhenAll(_instances.Select(i => i.DisposeAsync().AsTask()));

    public void ConfigureTestHostConfiguration(string testId, IConfigurationBuilder configBuilder)
    {
        configBuilder.Add("Messaging:MessageBroker:Host", "localhost");
        configBuilder.Add("Messaging:MessageBroker:Port", GetBrokerPortFor(testId));
        configBuilder.Add("Messaging:MessageBroker:User", "user");
        configBuilder.Add("Messaging:MessageBroker:Password", "password");
        configBuilder.Add("Messaging:MessageBroker:VirtualHost", testId);
    }

    public void ConfigureTestServices(string testId, IServiceCollection serviceCollection)
    {
        CreateTestVhost(testId);
    }

    public Task Clear(string testId) => DeleteTestVhostAsync(testId);

    public async Task DeleteExchangeAsync(string testId, string exchange)
    {
        var containerPort = GetManagementPortFor(testId);
        var response = await CreateHttpClient().DeleteAsync($"http://localhost:{containerPort}/api/exchanges/{testId}/{exchange}");
        response.EnsureSuccessStatusCode();
    }

    private async Task CreateTestContainers()
    {
        if (_instances.Count == _numberOfInstances)
            return;

        await _mutex.WaitAsync();

        if (_instances.Count == _numberOfInstances)
            return;

        _instances.AddRange(Enumerable
            .Range(0, _numberOfInstances)
            .Select(CreateInstance)
        );

        await Task.WhenAll(_instances.Select(i => i.StartAsync()));

        _mutex.Release();
    }

    private RabbitMqContainer CreateInstance(int instanceIdx)
    {
        var brokerPort = _defaultBrokerPort + instanceIdx;

        _portsAssigned.Add(brokerPort);

        var builder = new RabbitMqBuilder()
            .WithImage("rabbitmq:4.2.6-management-alpine")
            .WithHostname("localhost")
            .WithPortBinding(brokerPort, 5672)
            .WithPortBinding(_defaultManagementPort + instanceIdx, 15672)
            .WithUsername("user")
            .WithPassword("password");

        var instance = builder.Build();

        return instance;
    }
    private void CreateTestVhost(string testId)
    {
        if (!createTopologyConfig.ShouldCreateTopology(testId))
            return;

        var retryCount = 5;
        for (var i = 0; i < retryCount; i++)
        {
            try
            {
                var port = GetManagementPortFor(testId);
                var response = CreateHttpClient().Send(new HttpRequestMessage(HttpMethod.Post, $"http://localhost:{port}/api/definitions")
                {
                    Content = GetVhostCreationContent(testId)
                });
                response.EnsureSuccessStatusCode();

                return;
            }
            catch
            {
                if (i + 1 < retryCount)
                    continue;

                throw;
            }
        }
    }

    private static StringContent GetVhostCreationContent(string testId)
    {
        var def = GetDefinitionsJsonAsync();
        var replaced = ReplaceVhostWithTestId(def, testId);

        var content = new StringContent(replaced);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return content;
    }

    private static string GetDefinitionsJsonAsync()
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestSuite/Messaging/definitions.json"))
            ?? throw new ArgumentException("definitions.json could not be read.");
    }

    private static string ReplaceVhostWithTestId(string json, string testId)
    {
        return json
            .Replace(""""name": "/"""", $""""name": "{testId}"""")
            .Replace(""""vhost": "/"""", $""""vhost": "{testId}"""");
    }

    private async Task DeleteTestVhostAsync(string testId)
    {
        if (!createTopologyConfig.ShouldCreateTopology(testId))
            return;

        var containerPort = GetManagementPortFor(testId);
        var response = await CreateHttpClient().DeleteAsync($"http://localhost:{containerPort}/api/vhosts/{testId}");
        if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
            response.EnsureSuccessStatusCode();
    }

    private static string GetBrokerPortFor(string testId)
        => (_defaultBrokerPort + CalculateIndex(testId)).ToString();

    private static string GetManagementPortFor(string testId)
        => (_defaultManagementPort + CalculateIndex(testId)).ToString();

    private static int CalculateIndex(string testId)
    {
        var value = BitConverter.ToInt32(Guid.Parse(testId).ToByteArray(), 0);
        var instanceIndex = Math.Abs(value) % _numberOfInstances;

        return instanceIndex;
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        var credentials = Encoding.ASCII.GetBytes("user:password");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
        return client;
    }
}
