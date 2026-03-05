namespace AGVController;
using CommonAssetController;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

public class AGVController : IAssetController
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl;

    public AGVController(string baseUrl = "http://localhost:8082/v1" )

	{
        httpClient = new HttpClient();
        this.baseUrl = baseUrl;
    }

    public async Task<bool> Connect()
    {
        throw new NotImplementedException();
    }

    public Task<bool> Disconnect()
    {
        throw new NotImplementedException();
    }

    public async Task<string> ReadStatus()
    {
        var response = await httpClient.GetAsync($"{baseUrl}/status");
        return await response.Content.ReadAsStringAsync();
    }

    public async Task SendCommand(string command)
    {
		throw new NotImplementedException();
	}
}
