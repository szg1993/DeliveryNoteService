namespace ApiTests;

public class TopologyMissingTests : DemoServiceApiTestBase
{
    private Exception? _error;

    public override async Task InitializeAsync()
    {
        MessagingFixture.CreateTopologyConfig.Disable(TestId);

        try
        {
            await base.InitializeAsync();
        }
        catch (Exception e)
        {
            _error = e;
        }
    }

    [Fact]
    public void TopologyMissing()
    {
        _error.Should().NotBeNull();
        _error!.Message.Should().Be("Failed to initialize web app.");
    }
}
