namespace ApiTests.TestSuite;

public abstract class DeliveryNoteServiceApiTestBase : ApiTestBase<Program, DeliveryNoteServiceApi>
{
    protected override DeliveryNoteServiceApi CreateApiWrapper(HttpClient client)
        => new(client);
}