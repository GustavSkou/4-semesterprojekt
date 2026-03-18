namespace WarehouseController;

using System.Net;
using CommonAssetController;

public class WarehouseController : IAssetController
{
    private HttpClient _httpClient = new HttpClient();
    private readonly string _url = "http://localhost:8081/Service.asmx";

    public AssetEnum GetAssetEnum()
    {
        return AssetEnum.warehouse;
    }

    public Task SendCommand(string command, string[] args)
    {
        throw new NotImplementedException();
    }

    public async Task SendCommand(AssetCommand command)
    {
        foreach (var item in command.Items ?? [])
        {
            string soapEnvelope = $@"
                <Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                    <Body>
                        <PickItem xmlns=""http://tempuri.org/"">
                            <trayId>{item.Id}</trayId>
                        </PickItem>
                    </Body>
                </Envelope>";

            var content = new StringContent(soapEnvelope, System.Text.Encoding.UTF8, "text/xml");
            var response = await _httpClient.PostAsync(_url, content);
            response.EnsureSuccessStatusCode();
        }
    }

    Task<bool> IAssetController.Connect()
    {
        return Task.FromResult(true);
    }

    Task<bool> IAssetController.Disconnect()
    {
        throw new NotImplementedException();
    }

    Task<string> IAssetController.ReadStatus()
    {
        throw new NotImplementedException();
    }
}
