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

    public async Task<bool> SendCommand(AssetCommand command)
    {
        switch (command.Name) 
        {
            case "MoveToChargerOperation":
                return await MoveToCharing(command);

            case "MoveToAssemblyOperation":
                return await MoveToAssembly(command);

            case "MoveToStorageOperation":
                return await MoveToWarehouse(command);

            case "PutAssemblyOperation":
                return await Putdown();

            case "PickAssemblyOperation":
                return await PickUp(new Queue<Item>(command.Items));

            case "PickWarehouseOperation":
                return await PickUp(new Queue<Item>(command.Items));

            case "PutWarehouseOperation":
                return await Putdown();

            case "test":
                ProductionEventHandler?.Invoke(this, new ProductionEvent()
                {
                    DateAndTime = DateTime.Now,
                    Description = "prod event test",
                    Source = "AGV",
                    Type = "prod event test",
                    Level = "prod event test"
                });
                Console.WriteLine("SUCCESS");
                return true;

            default:
                return false;     
        }
    }

    private async Task<bool> MoveToWarehouse(AssetCommand command)
    {
        await ExecuteMovementCommand(command.Name);
        return await WhileMoving();
    }

    private async Task<bool> MoveToAssembly(AssetCommand command)
    {
        await ExecuteMovementCommand(command.Name);
        return await WhileMoving();
    }

    private async Task<bool> MoveToCharing(AssetCommand command)
    {
        await ExecuteMovementCommand(command.Name);
        return await WhileMoving();
    }

    // pick
    private async Task<bool> PickUp(Queue<Item> items)
    {
        while (items.TryDequeue(out Item item))
        {
            // await pick up action
            _heldItems.Enqueue(item);
        }

        return true;
    }

    // put down all items held
    private async Task<bool> Putdown()
    {
        Queue<Item> putdownItems = new Queue<Item>();

        while (_heldItems.TryDequeue(out Item item))
        {
            putdownItems.Enqueue(item);
        }

        return true;
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