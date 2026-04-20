namespace ProductionHandlerPlugin;

using Common.Util;
using Common.Data;

using CommonAssetController;
using Common.ProductionDataSource;
using Common.Persistence;
using Common.Service;
using Common.PubSubDataSource;

public class ProductionHandler : IProductionDataSource , IPlugin
{
    private Dictionary<string, IAssetController> _controllerRegistry;
    public event EventHandler<ProductionEvent>? EventHandler; // raise event on this, to notify ProductionDataSource
    private OrderDTO? _currentOrder = null;
    private ProductionState _state = ProductionState.idle;
    private readonly SemaphoreSlim _productionGate = new(1, 1);

    public ProductionHandler()
    {
        OrderHandler.Instance.NewOrder += OnNewOrder;

        _controllerRegistry = new Dictionary<string, IAssetController>();

        foreach (IAssetController controller in GetAssetControllers())
        {
            controller.ProductionEventHandler += OnProductionEvent;
            controller.Connect().GetAwaiter().GetResult();
            _controllerRegistry.Add(controller.GetAssetName, controller);
        }

        _ = PopulateWarehouses();
    }

    private void Publish(ProductionEvent e)
    {
        EventHandler?.Invoke(this, e);

        var pubSubs = ServiceLocator.Instance.LocateAll<IPubSubDataSource>();
        if (pubSubs.Count > 0) 
            pubSubs[0].Publish(e);
    }

    private void EmitStep(string stage, string state, string message, string level = "low")
    {
        Console.WriteLine($"STEP_EVENT -> {stage}|{state}|{message}");
        Publish(new ProductionEvent
        {
            DateAndTime = DateTime.Now,
            Source = "production handler",
            Type = "step-status",
            Level = level,
            Description = $"{stage}|{state}|{message}"
        });
    }
    

    private void OnProductionEvent(object? sender, ProductionEvent e)
    {
        Console.WriteLine(e);
        Publish(e);
    }

    /// <summary>
    /// When a new order is added to orderhandler's queue, this is invoked
    /// </summary>
    private void OnNewOrder(object? sender, EventArgs e)
    {
        Console.WriteLine("New Order Event in ProductionHandler");

        if (_state != ProductionState.idle)
            return;

        if (OrderHandler.Instance.OrderQueue.Count > 0)
        {
            _currentOrder = OrderHandler.Instance.OrderQueue.Dequeue();
            _ = StartProduction();
        }
    }

    private void OnProductionComplete(ProductionEvent e)
    {
        _state = ProductionState.idle;
        Publish(e);

        if (OrderHandler.Instance.OrderQueue.Count > 0)
        {
            _currentOrder = OrderHandler.Instance.OrderQueue.Dequeue();
            _ = StartProduction();
        }
    }

    private async Task StartProduction()
    {
        await _productionGate.WaitAsync();
        try
        {
            if (_currentOrder == null)
                return;

            Console.WriteLine($"Starting production! order: {_currentOrder.Id}");
            _state = ProductionState.executing;

            EmitStep("website", "in-progress", $"Order {_currentOrder.Id} received");
            await Task.Delay(3000);
            EmitStep("website", "completed", $"Order {_currentOrder.Id} validated");

            await HandleProduction();

            OnProductionComplete(new ProductionEvent
            {
                DateAndTime = DateTime.Now,
                Description = $"Order {_currentOrder.Id} completed",
                Source = "production handler",
                Type = "order completed",
                Level = "low"
            });
        }
        finally
        {
            _productionGate.Release();
        }
    }

    private async Task HandleProduction()
    {
        try
        {
            if (_currentOrder == null)
                return;

            EmitStep("warehouse-receive", "in-progress", "Picking components from warehouses");
            foreach (var group in _currentOrder.Items.GroupBy(i => GetWarehouseForTray(i.TrayId)))
                await group.Key.SendCommand(new AssetCommand("PickItem", group.ToArray()));
            EmitStep("warehouse-receive", "completed", "Components picked");

            EmitStep("agv-to-assembly", "in-progress", "Transporting components to assembly");
            var agvToAssemblyStartedAt = DateTime.UtcNow;
            await GetController("agv").SendCommand(new AssetCommand("MoveToStorageOperation", null));
            await GetController("agv").SendCommand(new AssetCommand("PickWarehouseOperation", _currentOrder.Items));
            await GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null));
            await GetController("agv").SendCommand(new AssetCommand("PutAssemblyOperation", null));

            var agvToAssemblyElapsed = DateTime.UtcNow - agvToAssemblyStartedAt;
            if (agvToAssemblyElapsed < TimeSpan.FromSeconds(3))
                await Task.Delay(TimeSpan.FromSeconds(3) - agvToAssemblyElapsed);

            EmitStep("agv-to-assembly", "completed", "Components delivered to assembly");

            EmitStep("assembly", "in-progress", "Assembly started");
            await GetController("assembly").SendCommand(new AssetCommand("start", null));
            EmitStep("assembly", "completed", "Assembly finished");

            await GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null));

            EmitStep("agv-to-warehouse", "in-progress", "Picking assembled product and returning to warehouse");
            var agvReturnStartedAt = DateTime.UtcNow;
            await GetController("agv").SendCommand(new AssetCommand("PickAssemblyOperation", _currentOrder.Items));
            await GetController("agv").SendCommand(new AssetCommand("MoveToStorageOperation", null));
            await GetController("agv").SendCommand(new AssetCommand("PutWarehouseOperation", null));

            var agvReturnElapsed = DateTime.UtcNow - agvReturnStartedAt;
            if (agvReturnElapsed < TimeSpan.FromSeconds(3))
                await Task.Delay(TimeSpan.FromSeconds(3) - agvReturnElapsed);

            EmitStep("agv-to-warehouse", "completed", "Returned to warehouse");

            EmitStep("warehouse-delivery", "in-progress", "Inserting finished product into warehouse");
            await InsertFinishedProduct();
            await Task.Delay(3000);
            EmitStep("warehouse-delivery", "completed", "Inserted into warehouse");

            EmitStep("delivery", "in-progress", "Preparing outbound delivery");
            await Task.Delay(1000);
            EmitStep("delivery", "completed", "Out for delivery");
        }
        catch (Exception ex)
        {
            EmitStep("production", "error", ex.ToString(), "high");
            _state = ProductionState.paused;
            Console.WriteLine("Error Production paused");
        }
    }

    /// <summary>
    /// Returns a list of Warehouse, agv and assembly controllers.
    /// Which can be used though the geniaric interface IAssetController
    /// </summary>
    /// <returns></returns>
    private IReadOnlyList<IAssetController> GetAssetControllers()
    {
        return ServiceLocator.Instance.LocateAll<IAssetController>();
    }

    private IAssetController GetController(string assetName)
    {
        IAssetController controller;
        if (_controllerRegistry.TryGetValue(assetName, out controller))
        {
            return controller;
        }
        else
        {
            var iAssetController = GetAssetControllers();
            //Dictionary<AssetEnum, IAssetController> controllerRegistry = new Dictionary<AssetEnum, IAssetController>();

            foreach (IAssetController assetController in iAssetController)
            {
                _controllerRegistry.TryAdd(assetController.GetAssetName, assetController);
            }

            if (!_controllerRegistry.TryGetValue(assetName, out controller))
                throw new Exception();

            return controller;
        }
    }

    private IWarehouseController GetWarehouseForTray(int trayId)
    {
        return GetAssetControllers()
            .OfType<IWarehouseController>()
            .First(w => trayId >= w.MinTray && trayId <= w.MaxTray);
    }

    private async Task InsertFinishedProduct()
    {
        var warehouse5 = GetWarehouseForTray(41);
        bool hasSpace = await warehouse5.SendCommand(new AssetCommand("CheckSpace", null));
        if (!hasSpace)
        {
            Console.WriteLine("Warehouse 5 is full... Production paused. Resume when items are shipped.");
            _state = ProductionState.paused;
            return;
        }
        await warehouse5.SendCommand(new AssetCommand("InsertItem", null));
    }

    public async Task RefillWarehouse()
    {
        foreach (var w in GetAssetControllers().OfType<IWarehouseController>().Where(w => w.MaxTray <= 40))
            await w.SendCommand(new AssetCommand("Refill", null));    
    }

    public async Task PopulateWarehouses()
    {
        var components = ServiceLocator.Instance.LocateAll<IPersistence>()[0].GetComponents();
        
        foreach (var group in components.GroupBy(c => GetWarehouseForTray(c.TrayId)))
            await group.Key.SendCommand(new AssetCommand("Populate", group.ToArray()));
    }

    public void PluginStart()
    {
        
    }

    public void PluginDispose()
    {
        
    }
}
