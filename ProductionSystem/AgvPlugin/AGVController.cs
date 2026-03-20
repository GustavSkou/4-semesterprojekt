namespace AGVController;

using CommonAssetController;
using Common.Data;
using System.Net.Http;
using System.Text.Json;
using System.Text;

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

    public async Task<string> ReadStatus()
    {
        var response = await httpClient.GetAsync($"{baseUrl}/status");
        return await response.Content.ReadAsStringAsync();
    }

    static async Task PostAsync(HttpClient httpClient)
    {
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                Programname = "MoveToAssemblyOperation",
                State = 1
            }),
            Encoding.UTF8,
            "application/json");

        using HttpResponseMessage response = await httpClient.PostAsync(
            "todos",
            jsonContent);

        var jsonResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"{jsonResponse}\n");

        // Expected output:
        //   POST https://jsonplaceholder.typicode.com/todos HTTP/1.1
        //   {
        //     "userId": 77,
        //     "id": 201,
        //     "title": "write code sample",
        //     "completed": false
        //   }
    }

    public Task SendCommand(AssetCommand command)
    {
        switch (command.Name)
        {
            case "MoveToChargerOperation":
                return MoveToCharing();

            case "MoveToAssemblyOperation":
                return MoveToAssembly();

            case "MoveToStorageOperation":
                return MoveToWarehouse();

            case "PutAssemblyOperation":
                return Putdown();

            case "PickAssemblyOperation":
                return PickUp(new Queue<Item>(command.Items));

            case "PickWarehouseOperation":
                return PickUp(new Queue<Item>(command.Items));

            case "PutWarehouseOperation":
                return Putdown();

            default:
                return Task.CompletedTask;
        }

        throw new NotImplementedException();
    }

    private Task MoveToWarehouse()
    {
        throw new NotImplementedException();
    }

    private Task MoveToAssembly()
    {
        throw new NotImplementedException();
    }

    private Task MoveToCharing()
    {
        throw new NotImplementedException();
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