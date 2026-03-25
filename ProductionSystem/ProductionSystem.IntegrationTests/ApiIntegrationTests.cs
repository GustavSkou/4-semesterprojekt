using System.Net;
  using System.Net.Http.Json;
  using Xunit;

  public class ApiIntegrationTests
  {
      // Where the API should be running while we test.
      private static readonly string BaseUrl =
          Environment.GetEnvironmentVariable("PRODUCTION_BASE_URL") ?? "http://localhost:5027";

      [Fact]
      // Test 1: "Can we reach the server?"
      // We call GET /ProductionSystem/TEST and expect 200 + "test".
      public async Task TestEndpoint_ReturnsTest()
      {
          using var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
          var res = await client.GetAsync("/ProductionSystem/TEST");
          var body = await res.Content.ReadAsStringAsync();

          Assert.Equal(HttpStatusCode.OK, res.StatusCode);
          Assert.Contains("test", body, StringComparison.OrdinalIgnoreCase);
      }

      [Fact]
      // Test 2: "Does the server accept a valid order?"
      // We send a command with Name + Parameters and expect 200.
      public async Task CommandEndpoint_AcceptsOrder()
      {
          using var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
          var payload = new { Name = "order", Parameters = new { id = 1, items = new[] { 10, 11 } } };

          var res = await client.PostAsJsonAsync("/ProductionSystem/Command", payload);
          Assert.Equal(HttpStatusCode.OK, res.StatusCode);
      }

      [Fact]
      // Test 3: "Is Name required?"
      // We send a command without Name and expect 400 BadRequest.
      public async Task CommandEndpoint_RejectsMissingName()
      {
          using var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
          var payload = new { Parameters = new { } };

          var res = await client.PostAsJsonAsync("/ProductionSystem/Command", payload);
          Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
      }
  }
