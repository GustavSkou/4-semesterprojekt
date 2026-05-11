using System.Net;
using System.Text;
using System.Net.Http.Json;

namespace ProductionSystem.IntegrationTests;

[Collection("PluginHost integration collection")]
public class ApiIntegrationTests
{
    private readonly PluginHostFixture _fixture;

    public ApiIntegrationTests(PluginHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestEndpoint_ReturnsTest()
    {
        using var client = _fixture.CreateClient();
        var res = await client.GetAsync("/ProductionSystem/TEST");
        var body = await res.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Contains("test", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CommandEndpoint_AcceptsOrder()
    {
        using var client = _fixture.CreateClient();
        var payload = new { Name = "order", Parameters = new { id = 1, items = new[] { 10, 11 } } };

        var res = await client.PostAsJsonAsync("/ProductionSystem/Command", payload);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task CommandEndpoint_RejectsMissingName()
    {
        using var client = _fixture.CreateClient();
        var payload = new { Parameters = new { } };

        var res = await client.PostAsJsonAsync("/ProductionSystem/Command", payload);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    // This integration test verifies that the queue endpoint responds successfully.
    // It checks that the API can return the current production queue information.

    // The test sends a GET request to the queue endpoint
    // and verifies that the response status is successful.

    // This supports the user story about monitoring production
    // and viewing production status information.
    [Fact]
    public async Task QueueEndpoint_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/ProductionSystem/Queue");

        response.EnsureSuccessStatusCode();

        Assert.True(response.IsSuccessStatusCode);
    }

    // This integration test supports the operator user story about stopping production.
    // It verifies that the API accepts a stop command and returns a successful response.
    //
    // This is important because the operator must be able to stop production
    // when an error occurs or when production needs to be paused safely.
    [Fact]
    public async Task StopEndpoint_ReturnsSuccessStatusCode()
    {
        using var client = _fixture.CreateClient();

        var res = await client.PostAsync("/ProductionSystem/Stop", null);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    // This integration test supports the operator user story about resetting production.
    // It verifies that the API accepts a reset command and returns a successful response.
    //
    // This is important because the operator must be able to reset the system
    // so production can return to a controlled starting state.
    [Fact]
    public async Task ResetEndpoint_ReturnsSuccessStatusCode()
    {
        using var client = _fixture.CreateClient();

        var res = await client.PostAsync("/ProductionSystem/Reset", null);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    // This integration test supports the operator user story about starting production.
    // It verifies that the API accepts a start command and returns a successful response.
    //
    // This is important because the operator must be able to start or resume production
    // after the system has been stopped or reset.
    [Fact]
    public async Task StartEndpoint_ReturnsSuccessStatusCode()
    {
        using var client = _fixture.CreateClient();

        var res = await client.PostAsync("/ProductionSystem/Start", null);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    // This integration test supports the user story about live tracking of production.
    // It verifies that the API can return information about the production machines.
    //
    // The test sends a GET request to the Machines endpoint,
    // checks that the response is successful,
    // and checks that the response body contains machine data.
    //
    // This is important because the operator must be able to monitor
    // the production assets during the production process.
    [Fact]
    public async Task MachinesEndpoint_ReturnsMachineStatusData()
    {
        using var client = _fixture.CreateClient();

        var res = await client.GetAsync("/ProductionSystem/Machines");
        var body = await res.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Contains("machines", body, StringComparison.OrdinalIgnoreCase);
    }

    // This integration test supports the user story about automatically starting production
    // after a configuration has been confirmed.
    //
    // The test sends a production order request to the API
    // and verifies that the system accepts the request successfully.
    //
    // This is important because the production flow depends on the API
    // correctly receiving and processing incoming production orders.
    [Fact]
    public async Task CommandEndpoint_AcceptsValidOrder()
    {
        using var client = _fixture.CreateClient();

        var json =
            """
            {
                "id": 1,
                "items": [1, 2, 3]
            }
            """;

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var res = await client.PostAsync(
            "/ProductionSystem/Command",
            content
        );

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    // This integration test verifies that the system rejects invalid production orders.
    // The test sends an incomplete JSON request to the API
    // and checks that the API responds with a bad request status.
    //
    // This is important because the production system must not accept
    // invalid or incomplete production data.
    [Fact]
    public async Task CommandEndpoint_ReturnsBadRequest_ForInvalidOrder()
    {
        using var client = _fixture.CreateClient();

        var json =
            """
            {
                "invalidData": true
            }
            """;

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var res = await client.PostAsync(
            "/ProductionSystem/Command",
            content
        );

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
