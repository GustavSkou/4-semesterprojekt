using System.Net;
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
}
