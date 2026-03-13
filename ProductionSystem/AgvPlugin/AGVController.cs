namespace AGVController;

using CommonAssetController;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

public class AGVController : IAssetController
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl = "http://localhost:8082/v1";

    public AGVController()
	{
        httpClient = new HttpClient();
    }

    public async Task<bool> Connect()
    {
        throw new NotImplementedException();
    }

    public Task<bool> Disconnect()
    {
        throw new NotImplementedException();
    }

    public AssetEnum GetAssetEnum()
    {
        return AssetEnum.agv;
    }

    public async Task<string> ReadStatus()
    {
        var response = await httpClient.GetAsync($"{baseUrl}/status");
        return await response.Content.ReadAsStringAsync();
    }

    public Task SendCommand(string command, string[] args)
    {
        throw new NotImplementedException();
    }

    public Task SendCommand(AssetCommand command)
    {
        throw new NotImplementedException();
    }
}
