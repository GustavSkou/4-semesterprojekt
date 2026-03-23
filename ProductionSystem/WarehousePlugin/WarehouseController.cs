namespace WarehouseController;

using Common.Data;
using CommonAssetController;
using System.Net;

public class WarehouseController : IAssetController
{
    public event EventHandler<ProductionEvent>? ProductionEventHandler;

    private HttpClient _httpClient = new HttpClient();
    private readonly string _url = "http://localhost:8081/Service.asmx";

    public string GetAssetName { get { return "warehouse"; } }

    public async Task SendCommand(AssetCommand command)
    {
        foreach (var item in command.Items ?? [])
        {
            string soapEnvelope = $@"
                <Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
                    <Body>
                        <PickItem xmlns=""http://tempuri.org/"">
                            <trayId>{item.TrayId}</trayId>
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

    Task<string> ReadStatus()
    {
        throw new NotImplementedException();
    }
}
