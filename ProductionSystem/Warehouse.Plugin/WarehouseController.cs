namespace WarehouseController;

using Common.Data;
using CommonAssetController;
using System.Text.Json;
using ServiceReference1;

public class WarehouseController : IAssetController
{
    public event EventHandler<ProductionEvent>? ProductionEventHandler;

    private readonly EmulatorServiceClient _client = new EmulatorServiceClient();

    public string GetAssetName { get { return "warehouse"; } }

    public async Task<bool> SendCommand(AssetCommand command)
    {
        switch (command.Name)
        {
            case "PickItem":
            {
                var emptyIds = await GetEmptyTrayIds();
                var emptyRequested = command.Items?
                    .Select(i => i.TrayId)
                    .Where(id => emptyIds.Contains(id))
                    .ToList() ?? [];

                if (emptyRequested.Count > 0)
                {
                    Console.WriteLine($"Components missing in tray {string.Join(", ", emptyRequested)} -> refilling now...");
                    await SendCommand(new AssetCommand("refill", emptyRequested.Select(id => new Item { TrayId = id }).ToArray()));
                    await Task.Delay(3000);
                }

                foreach (var item in command.Items ?? [])
                {
                    await _client.PickItemAsync(item.TrayId);
                    Console.WriteLine($"Picked item from tray {item.TrayId}");
                    await Task.Delay(1000);
                }
                return true;
            }
            
            case "refill":
            {
                var idsToRefill = command.Items?.Length > 0
                    ? command.Items.Select(i => i.TrayId).ToList()
                    : await GetEmptyTrayIds();

                if (idsToRefill.Count == 0)
                    return true;

                Console.WriteLine($"Refilling slots: {string.Join(", ", idsToRefill)}");

                foreach (var id in idsToRefill)
                {
                    await _client.InsertItemAsync(id, $"Item {id}");
                    Console.WriteLine($"Refilled item in tray {id}");
                    await Task.Delay(1000);
                } 

                return true;
            }
            default:
                return true;
        } 
    }

    private async Task<List<int>> GetEmptyTrayIds()
    {
        string json = await _client.GetInventoryAsync();
        
        var emptyIds = JsonDocument.Parse(json)
            .RootElement.GetProperty("Inventory")
            .EnumerateArray()
            .Where(x => string.IsNullOrEmpty(x.GetProperty("Content").GetString()))
            .Select(x => x.GetProperty("Id").GetInt32())
            .ToList();

        return emptyIds;
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
