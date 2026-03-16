namespace AGVController;

using CommonAssetController;
using Common.Data;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;
using System.Security.AccessControl;
using System.Net.ServerSentEvents;

public class AGVController : IAssetController
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl = "http://localhost:8082/v1";

    private Queue<Item> _heldItems;

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

    private Task MoveToWarehouse()
    {
        throw new NotImplementedException();
    }

    private Task MoveToAssembly()
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

    public Task SendCommand(AssetCommand command)
    {
        throw new NotImplementedException();
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