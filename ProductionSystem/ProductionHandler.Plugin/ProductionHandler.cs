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
    private readonly IReadOnlyList<IAssetController> _assetControllers;
    private readonly IReadOnlyList<IPersistence> _persistenceServices;
    private readonly IReadOnlyList<IPubSubDataSource> _pubSubServices;
    private readonly Func<TimeSpan, Task> _delay;
    private readonly bool _subscribedToOrderHandler;

    public ProductionHandler()
        : this(
            assetControllers: null,
            persistenceServices: null,
            pubSubServices: null,
            delay: null,
            subscribeToOrderHandler: true,
            populateWarehousesOnStart: true)
    {
    }

    public ProductionHandler(
        IEnumerable<IAssetController>? assetControllers,
        IEnumerable<IPersistence>? persistenceServices = null,
        IEnumerable<IPubSubDataSource>? pubSubServices = null,
        Func<TimeSpan, Task>? delay = null,
        bool subscribeToOrderHandler = true,
        bool populateWarehousesOnStart = true)
    {
        _assetControllers = assetControllers?.ToList() ?? GetAssetControllersFromServices();
        _persistenceServices = persistenceServices?.ToList() ?? GetPersistenceServices();
        _pubSubServices = pubSubServices?.ToList() ?? GetPubSubServices();
        _delay = delay ?? Task.Delay;
        _subscribedToOrderHandler = subscribeToOrderHandler;

        if (_subscribedToOrderHandler)
            OrderHandler.Instance.NewOrder += OnNewOrder;

        _controllerRegistry = new Dictionary<string, IAssetController>();

        foreach (IAssetController controller in _assetControllers)
        {
            controller.ProductionEventHandler += OnProductionEvent;
            _controllerRegistry.Add(controller.GetAssetName, controller);

            try
            {
                var connected = controller.Connect().GetAwaiter().GetResult();
                if (!connected)
                    Console.WriteLine($"Asset controller '{controller.GetAssetName}' is unavailable during startup.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Asset controller '{controller.GetAssetName}' failed during startup: {ex}");
            }
        }

        if (populateWarehousesOnStart && _persistenceServices.Count > 0)
            _ = PopulateWarehouses();
    }

    private void Publish(ProductionEvent e)
    {
        EventHandler?.Invoke(this, e);

        if (_pubSubServices.Count > 0)
            _pubSubServices[0].Publish(e);
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

    private void EmitControl(string action, string message, string level = "medium")
    {
        Publish(new ProductionEvent
        {
            DateAndTime = DateTime.Now,
            Source = "control",
            Type = "control",
            Level = level,
            Description = $"{action}|{message}"
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

    private async Task StartProduction()
    {
        await _productionGate.WaitAsync();
        try
        {
            while (_currentOrder != null)
            {
                var completedOrderId = _currentOrder.Id;

                Console.WriteLine($"Starting production! order: {completedOrderId}");
                _state = ProductionState.executing;

                EmitStep("website", "in-progress", $"Order {completedOrderId} received");
                await _delay(TimeSpan.FromSeconds(3));
                EmitStep("website", "completed", $"Order {completedOrderId} validated");

                var completedSuccessfully = await HandleProduction();
                if (!completedSuccessfully)
                    return;

                _state = ProductionState.idle;
                Publish(new ProductionEvent
                {
                    DateAndTime = DateTime.Now,
                    Description = $"Order {completedOrderId} completed",
                    Source = "production handler",
                    Type = "order completed",
                    Level = "low"
                });

                _currentOrder = OrderHandler.Instance.OrderQueue.Count > 0
                    ? OrderHandler.Instance.OrderQueue.Dequeue()
                    : null;
            }
        }
        finally
        {
            _productionGate.Release();
        }
    }

    private async Task<bool> HandleProduction()
    {
        try
        {
            if (_currentOrder == null)
                return false;

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
                await _delay(TimeSpan.FromSeconds(3) - agvToAssemblyElapsed);

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
                await _delay(TimeSpan.FromSeconds(3) - agvReturnElapsed);

            EmitStep("agv-to-warehouse", "completed", "Returned to warehouse");

            EmitStep("warehouse-delivery", "in-progress", "Inserting finished product into warehouse");
            var insertedFinishedProduct = await InsertFinishedProduct();
            if (!insertedFinishedProduct)
                return false;
            await _delay(TimeSpan.FromSeconds(3));
            EmitStep("warehouse-delivery", "completed", "Inserted into warehouse");

            EmitStep("delivery", "in-progress", "Preparing outbound delivery");
            await _delay(TimeSpan.FromSeconds(1));
            EmitStep("delivery", "completed", "Out for delivery");
            return true;
        }
        catch (Exception ex)
        {
            EmitStep("production", "error", ex.ToString(), "high");
            _state = ProductionState.paused;
            Console.WriteLine("Error Production paused");
            return false;
        }
    }

    /// <summary>
    /// Returns a list of Warehouse, agv and assembly controllers.
    /// Which can be used though the geniaric interface IAssetController
    /// </summary>
    /// <returns></returns>
    private IReadOnlyList<IAssetController> GetAssetControllers()
    {
        return _assetControllers;
    }

    private static IReadOnlyList<IAssetController> GetAssetControllersFromServices()
    {
        return ServiceLocator.Instance.LocateAll<IAssetController>();
    }

    private static IReadOnlyList<IPersistence> GetPersistenceServices()
    {
        return ServiceLocator.Instance.LocateAll<IPersistence>();
    }

    private static IReadOnlyList<IPubSubDataSource> GetPubSubServices()
    {
        return ServiceLocator.Instance.LocateAll<IPubSubDataSource>();
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

    private async Task<bool> InsertFinishedProduct()
    {
        var warehouse5 = GetWarehouseForTray(41);
        bool hasSpace = await warehouse5.SendCommand(new AssetCommand("CheckSpace", null));
        if (!hasSpace)
        {
            Console.WriteLine("Warehouse 5 is full... Production paused. Resume when items are shipped.");
            _state = ProductionState.paused;
            return false;
        }
        await warehouse5.SendCommand(new AssetCommand("InsertItem", null));
        return true;
    }

    public async Task RefillWarehouse()
    {
        foreach (var w in GetAssetControllers().OfType<IWarehouseController>().Where(w => w.MaxTray <= 40))
            await w.SendCommand(new AssetCommand("Refill", null));    
    }

    public async Task PopulateWarehouses()
    {
        var persistence = _persistenceServices.FirstOrDefault();
        if (persistence == null)
            return;

        var components = persistence.GetComponents();
        
        foreach (var group in components.GroupBy(c => GetWarehouseForTray(c.TrayId)))
            await group.Key.SendCommand(new AssetCommand("Populate", group.ToArray()));
    }

    public Task Stop()
    {
        _state = ProductionState.paused;
        EmitControl("stop", "Production stopped by operator", "warning");
        return Task.CompletedTask;
    }

    public Task Reset()
    {
        OrderHandler.Instance.OrderQueue.Clear();
        _currentOrder = null;
        _state = ProductionState.idle;
        EmitControl("reset", "Production reset by operator");
        return Task.CompletedTask;
    }

    public Task Resume()
    {
        if (_state == ProductionState.executing)
            return Task.CompletedTask;

        _state = ProductionState.idle;
        EmitControl("resume", "Production resumed by operator", "low");

        if (_currentOrder == null && OrderHandler.Instance.OrderQueue.Count > 0)
            _currentOrder = OrderHandler.Instance.OrderQueue.Dequeue();

        if (_currentOrder != null)
            _ = StartProduction();

        return Task.CompletedTask;
    }

    public void PluginStart()
    {
        
    }

    public void PluginDispose()
    {
        if (_subscribedToOrderHandler)
            OrderHandler.Instance.NewOrder -= OnNewOrder;
    }
}
