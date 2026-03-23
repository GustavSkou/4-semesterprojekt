namespace AGVController;

using CommonAssetController;
using Common.Data;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.ComponentModel.Design;

public class AGVController : IAssetController
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl = "http://localhost:8082/v1";

    private Queue<Item> _heldItems;

    public event EventHandler<ProductionEvent>? ProductionEventHandler;

    public AGVController()
    {
        httpClient = new HttpClient();
        _heldItems = new Queue<Item>();
    }

    public async Task<bool> Connect()
    {
        throw new NotImplementedException();
    }

    public Task<bool> Disconnect()
    {
        throw new NotImplementedException();
    }

    public string GetAssetName { get { return "agv"; } }

    private async Task<StatusDTO?> ReadStatus()
    {
        var response = await httpClient.GetAsync($"{baseUrl}/status");
        StatusDTO? status = JsonSerializer.Deserialize<StatusDTO>(await response.Content.ReadAsStringAsync());

        return status;
    }

    private async Task LoadProgramAsync(string programName)
    {
        var payload = new Dictionary<string, string>
        {
            ["Program name"] = programName
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PutAsync($"{baseUrl}/status", content);
        response.EnsureSuccessStatusCode();
    }

    private async Task LoadExecuteStateAsync()
    {
        var payload = new Dictionary<string, string>
        {
            ["state"] = "2"
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PutAsync($"{baseUrl}/status", content);
        response.EnsureSuccessStatusCode();
    }

    public Task SendCommand(AssetCommand command)
    {
        switch (command.Name)
        {
            case "MoveToChargerOperation":
                return MoveToCharing(command);

            case "MoveToAssemblyOperation":
                return MoveToAssembly(command);

            case "MoveToStorageOperation":
                return MoveToWarehouse(command);

            case "PutAssemblyOperation":
                return Putdown();

            case "PickAssemblyOperation":
                return PickUp(new Queue<Item>(command.Items));

            case "PickWarehouseOperation":
                return PickUp(new Queue<Item>(command.Items));

            case "PutWarehouseOperation":
                return Putdown();

            case "test":
                Console.WriteLine("SUCCESS");
                break;

            default:
                return Task.CompletedTask;     
        }
        return Task.CompletedTask;
    }

    private async Task<bool> MoveToWarehouse(AssetCommand command)
    {
        await Move(command.Name);
        return await WhileMoving();
    }

    private async Task<bool> Move(string programName)
    {
        var status = await ReadStatus();

        if (status == null)
            return false;

        if (status.State != 1)
            return false;
        
        await LoadProgramAsync(programName);
        await LoadExecuteStateAsync();
        return true;
    }

    private async Task<bool> WhileMoving()
    {
        try {
            while ((await ReadStatus()).State == 2) {
                Thread.Sleep(250);
            }
            return true;
        }
        catch (NullReferenceException ex) {
            return false;
        }
    }

    private async Task<bool> MoveToAssembly(AssetCommand command)
    {
        await Move(command.Name);
        return await WhileMoving();
    }

    private async Task<bool> MoveToCharing(AssetCommand command)
    {
        await Move(command.Name);
        return await WhileMoving();
    }

    // pick
    private Task PickUp(Queue<Item> items)
    {
        while (items.TryDequeue(out Item item))
        {
            // await pick up action
            _heldItems.Enqueue(item);
        }

        return Task.CompletedTask;
    }

    // put down all items held
    private Task<Queue<Item>> Putdown()
    {
        Queue<Item> putdownItems = new Queue<Item>();

        while (_heldItems.TryDequeue(out Item item))
        {
            putdownItems.Enqueue(item);
        }

        return Task.FromResult(putdownItems);
    }

}


/*

MoveToChargerOperation  - Move the AGV to the charging station.

MoveToAssemblyOperation - Move the AGV to the assembly station.

MoveToStorageOperation  - Move the AGV to the warehouse.

PutAssemblyOperation    - Activate the robot arm to pick payload from AGV and place it at the assembly station.

PickAssemblyOperation   - Activate the robot arm to pick payload at the assembly station and place it on the AGV.

PickWarehouseOperation  - Activate the robot arm to pick payload from the warehouse outlet.

PutWarehouseOperation   - Activate the robot arm to place an item at the warehouse inlet.

*/