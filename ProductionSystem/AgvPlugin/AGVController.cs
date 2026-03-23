namespace AGVController;

using CommonAssetController;
using Common.Data;
using System.Net.Http;


public partial class AGVController : IAssetController
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

    public string GetAssetName { get { return "agv"; } }

    public async Task<bool> Connect()
    {
        return true;
    }

    public async Task<bool> Disconnect()
    {
        return true;
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