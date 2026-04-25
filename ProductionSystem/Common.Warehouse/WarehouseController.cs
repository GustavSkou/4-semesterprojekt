namespace WarehouseController;

using Common.Data;
using CommonAssetController;
using System.Text.Json;
using ServiceReference1;

public abstract class WarehouseController : IWarehouseController
{
    public event EventHandler<ProductionEvent>? ProductionEventHandler;

    private  EmulatorServiceClient Client => new EmulatorServiceClient(
        EmulatorServiceClient.EndpointConfiguration.BasicHttpBinding_IEmulatorService, Url);

    protected abstract string Url { get; }
    public abstract string GetAssetName { get; }

    public abstract int MinTray { get; }
    public abstract int MaxTray { get; }

    protected virtual bool ClearOnConnect => false;

    private int ToLocalTray(int globalId) => globalId - (MinTray - 1);
    private int ToGlobalTray(int localId) => localId + (MinTray - 1);

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
                    await SendCommand(new AssetCommand("Refill", emptyRequested.Select(id => new Item { TrayId = id }).ToArray()));
                    await Task.Delay(3000);
                }

                foreach (var item in command.Items ?? [])
                {
                    await Client.PickItemAsync(ToLocalTray(item.TrayId));
                    ProductionEventHandler?.Invoke(this, new ProductionEvent() {
                        DateAndTime = DateTime.Now,
                        Description = $"Picked item from tray {item.TrayId}",
                        Source = GetAssetName,
                        Type = "command",
                        Level = "low"
                    });
                    //Console.WriteLine($"Picked item from tray {item.TrayId}");
                    await Task.Delay(1000);
                }
                return true;
            }

            case "InsertItem":
            {
                var emptyIds = await GetEmptyTrayIds();
                if (emptyIds.Count == 0)
                    return false;

                int globalId = emptyIds.First();
                await Client.InsertItemAsync(ToLocalTray(globalId), "Finished PC");
                
                ProductionEventHandler?.Invoke(this, new ProductionEvent() {
                    DateAndTime = DateTime.Now,
                    Description = $"Inserted finished product into tray {globalId}",
                    Source = GetAssetName,
                    Type = "command",
                    Level = "low"
                });

                //Console.WriteLine($"Inserted finished product into tray {globalId}");
                return true;
            }
            
            case "Refill":
            {
                var idsToRefill = command.Items?.Length > 0
                    ? command.Items.Select(i => i.TrayId).ToList()
                    : await GetEmptyTrayIds();

                if (idsToRefill.Count == 0)
                    return true;

                Console.WriteLine($"Refilling slots: {string.Join(", ", idsToRefill)}");

                foreach (var id in idsToRefill)
                {
                    await Client.InsertItemAsync(ToLocalTray(id), $"Item {id}");

                    ProductionEventHandler?.Invoke(this, new ProductionEvent() {
                        DateAndTime = DateTime.Now,
                        Description = $"Refilled item in tray {id}",
                        Source = GetAssetName,
                        Type = "command",
                        Level = "low"
                    });
                    await Task.Delay(1000);
                } 

                return true;
            }

            case "CheckSpace":
            {
                var emptyIds = await GetEmptyTrayIds();
                return emptyIds.Count > 0;
            }

            case "Clear":
            {
                string json = await Client.GetInventoryAsync();
                var filledIds = JsonDocument.Parse(json)
                    .RootElement.GetProperty("Inventory")
                    .EnumerateArray()
                    .Where(x => !string.IsNullOrEmpty(x.GetProperty("Content").GetString()))
                    .Select(x => x.GetProperty("Id").GetInt32())
                    .ToList();

                foreach (var id in filledIds)
                    await Client.PickItemAsync(id);

                Console.WriteLine($"Cleared {filledIds.Count} trays");
                return true;
            }

            case "Populate":
            {
                List<int> emptyIds = await GetEmptyTrayIds();
                foreach (var item in command.Items ?? [])
                {
                    if (!emptyIds.Contains(item.TrayId))
                        continue;

                    await Client.InsertItemAsync(ToLocalTray(item.TrayId), item.Name ?? $"Item {item.TrayId}");
                    Console.WriteLine($"Populated tray {item.TrayId} with {item.Name}");
                }
                return true;
            }

            default:
                return true;
        } 
    }

    private async Task<List<int>> GetEmptyTrayIds()
    {
        string json = await Client.GetInventoryAsync();
        
        var emptyIds = JsonDocument.Parse(json)
            .RootElement.GetProperty("Inventory")
            .EnumerateArray()
            .Where(x => string.IsNullOrEmpty(x.GetProperty("Content").GetString()))
            .Select(x => ToGlobalTray(x.GetProperty("Id").GetInt32()))
            .ToList();

        return emptyIds;
    }

    public virtual async Task<bool> Connect()
    {
        if (ClearOnConnect)
        {
            bool isSeeded = await HasExpectedSeedInFirstTray();
            if (!isSeeded)
                await SendCommand(new AssetCommand("Clear", null));
        }

        return true;
    }

    private async Task<bool> HasExpectedSeedInFirstTray()
    {
        string json = await Client.GetInventoryAsync();
        JsonElement tray = JsonDocument.Parse(json)
            .RootElement
            .GetProperty("Inventory")
            .EnumerateArray()
            .FirstOrDefault(x => x.GetProperty("Id").GetInt32() == 1);

        if (tray.ValueKind == JsonValueKind.Undefined)
            return false;

        string? content = tray.GetProperty("Content").GetString();
        string expected = $"Item {MinTray}";
        return string.Equals(content, expected, StringComparison.OrdinalIgnoreCase);
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
