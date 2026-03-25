namespace AGVController;

using CommonAssetController;
using Common.Data;
using System.Net.Http;


public partial class AGVController : IAssetController
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl = "http://localhost:8082/v1";

    private List<Item> _heldItems;

    public event EventHandler<ProductionEvent>? ProductionEventHandler;

    public AGVController()
    {
        httpClient = new HttpClient();
        _heldItems = new List<Item>();
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
                return await Putdown(command);

            case "PickAssemblyOperation":
                if (command.Items == null)
                    return false;

                return await PickUp(command);

            case "PickWarehouseOperation":
                if (command.Items == null)
                    return false;

                return await PickUp(command);

            case "PutWarehouseOperation":
                return await Putdown(command);

            case "test":
                ProductionEventHandler?.Invoke(this, new ProductionEvent()
                {
                    DateAndTime = DateTime.Now,
                    Description = "prod event test",
                    Source = GetAssetName,
                    Type = "prod event test",
                    Level = "prod event test"
                });
                return true;

            default:
                return false;     
        }
    }

    private async Task<bool> MoveToWarehouse(AssetCommand command)
    {
        await ExecuteCommand(command.Name);
        return await WhileDoing();
    }

    private async Task<bool> MoveToAssembly(AssetCommand command)
    {
        await ExecuteCommand(command.Name);
        return await WhileDoing();
    }

    

    private async Task<bool> MoveToCharing(AssetCommand command)
    {
        await ExecuteCommand(command.Name);
        return await WhileDoing();
    }

    // pick
    private async Task<bool> PickUp(AssetCommand command)
    {
        foreach (var item in command.Items)
        {
            await ExecuteCommand(command.Name);
            await WhileDoing();
            _heldItems.Add(item);
        }
        return true;
    }

    // put down all items held
    private async Task<bool> Putdown(AssetCommand command)
    {
        foreach (var item in _heldItems)
        {
            await ExecuteCommand(command.Name);
            await WhileDoing();
        }
        _heldItems.Clear();
        return true;
    }

    private void ExecuteCommandEvent(string commandName)
    {
        ProductionEventHandler?.Invoke(this, new ProductionEvent()
        {
            DateAndTime = DateTime.Now,
            Description = commandName,
            Source = GetAssetName,
            Type = "command",
            Level = "low"
        });
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