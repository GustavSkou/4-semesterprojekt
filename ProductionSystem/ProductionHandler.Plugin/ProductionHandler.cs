namespace ProductionHandlerPlugin;

using Common.Util;
using Common.Data;

using CommonAssetController;
using Common.ProductionDataSource;
using Common.Persistence;
using Common.Service;
using Common.PubSubDataSource;
using System.Reflection.PortableExecutable;
using System.Data.Common;

public class ProductionHandler : IProductionDataSource, IProductionSnapshotSource, IPlugin
{
    private sealed class MachineSnapshot
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string ConnectionStatus { get; set; } = "disconnected";
        public string State { get; set; } = "offline";
        public string CurrentTask { get; set; } = "Connection unavailable";
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }

    private sealed class OrderStatusSnapshot
    {
        public int OrderId { get; set; }
        public string Stage { get; set; } = "website";
        public string State { get; set; } = "pending";
        public string Message { get; set; } = "";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    private readonly Dictionary<string, IAssetController> _controllerRegistry;
    private readonly Dictionary<string, MachineSnapshot> _machineSnapshots;
    private readonly Dictionary<int, OrderStatusSnapshot> _orderStatusByOrderId;

    public event EventHandler<ProductionEvent>? EventHandler;
    private OrderDTO? _currentOrder = null;
    private ProductionState _state = ProductionState.idle;

    private bool _stopRequested = false;
    private readonly SemaphoreSlim _productionGate = new(1, 1);

    public ProductionHandler()
    {
        OrderHandler.Instance.NewOrder += OnNewOrder;

        _controllerRegistry = new Dictionary<string, IAssetController>(StringComparer.OrdinalIgnoreCase);
        _machineSnapshots = new Dictionary<string, MachineSnapshot>(StringComparer.OrdinalIgnoreCase);
        _orderStatusByOrderId = new Dictionary<int, OrderStatusSnapshot>();

        foreach (IAssetController controller in GetAssetControllers())
        {
            bool connected = false;
            controller.ProductionEventHandler += OnProductionEvent;

            try
            {
                connected = controller.Connect().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{controller.GetAssetName} connect failed: {ex.Message}");
            }

            _controllerRegistry[controller.GetAssetName] = controller;
            RegisterMachineSnapshot(controller.GetAssetName, connected);
        }

        _ = PopulateWarehouses();
    }

    private static string GetMachineType(string source)
    {
        if (source.StartsWith("warehouse", StringComparison.OrdinalIgnoreCase))
            return "warehouse";

        if (source.StartsWith("agv", StringComparison.OrdinalIgnoreCase))
            return "agv";

        if (source.StartsWith("assembly", StringComparison.OrdinalIgnoreCase))
            return "assembly";

        return "unknown";
    }

    private static string GetMachineName(string source)
    {
        if (source.StartsWith("warehouse", StringComparison.OrdinalIgnoreCase))
            return $"Warehouse {source.Replace("warehouse", "")}";

        if (source.StartsWith("agv", StringComparison.OrdinalIgnoreCase))
            return "AGV Transport";

        if (source.StartsWith("assembly", StringComparison.OrdinalIgnoreCase))
            return "Assembly Station";

        return source;
    }

    private void RegisterMachineSnapshot(string source, bool connected)
    {
        string id = source.ToLowerInvariant();

        _machineSnapshots[id] = new MachineSnapshot
        {
            Id = id,
            Name = GetMachineName(id),
            Type = GetMachineType(id),
            ConnectionStatus = connected ? "connected" : "disconnected",
            State = connected ? "idle" : "offline",
            CurrentTask = connected ? "Standby" : "Connection unavailable",
            LastUpdatedAt = DateTime.UtcNow
        };
    }

    private void Publish(ProductionEvent e)
    {
        EventHandler?.Invoke(this, e);

        IReadOnlyList<IPubSubDataSource> pubSubs = ServiceLocator.Instance.LocateAll<IPubSubDataSource>();
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

        if (_currentOrder != null)
        {
            _orderStatusByOrderId[_currentOrder.Id] = new OrderStatusSnapshot
            {
                OrderId = _currentOrder.Id,
                Stage = stage,
                State = state,
                Message = message,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

    private void UpdateMachineFromEvent(ProductionEvent e)
    {
        string source = (e.Source ?? "").Trim().ToLowerInvariant();
        string type = (e.Type ?? "").Trim().ToLowerInvariant();
        string description = (e.Description ?? "").Trim();

        if (string.IsNullOrWhiteSpace(source))
            return;

        if (!_machineSnapshots.TryGetValue(source, out MachineSnapshot? machine))
        {
            machine = new MachineSnapshot
            {
                Id = source,
                Name = GetMachineName(source),
                Type = GetMachineType(source)
            };

            _machineSnapshots[source] = machine;
        }

        machine.ConnectionStatus = "connected";
        machine.LastUpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(description))
            machine.CurrentTask = description;

        if (type == "error" || string.Equals(e.Level, "high", StringComparison.OrdinalIgnoreCase))
        {
            machine.State = "error";
            return;
        }

        if (type == "step-status")
        {
            if (description.Contains("|completed|", StringComparison.OrdinalIgnoreCase))
                machine.State = "idle";
            else if (description.Contains("|in-progress|", StringComparison.OrdinalIgnoreCase))
                machine.State = "working";
            else if (description.Contains("|error|", StringComparison.OrdinalIgnoreCase))
                machine.State = "error";

            return;
        }

        machine.State = "working";
    }

    private void OnProductionEvent(object? sender, ProductionEvent e)
    {
        Console.WriteLine(e);
        UpdateMachineFromEvent(e);
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

    public async Task StartProduction()
    {
        await _productionGate.WaitAsync();
        try
        {
            if (_currentOrder == null)
                return;

            Console.WriteLine($"Starting production! order: {_currentOrder.Id}");
            _state = ProductionState.executing;
            _stopRequested = false;

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

            if (_stopRequested)
                return;

            EmitStep("warehouse-receive", "in-progress", "Picking components from warehouses");
            foreach (IGrouping<IWarehouseController, Item> group in _currentOrder.Items.GroupBy(i => GetWarehouseForTray(i.TrayId)))
                await group.Key.SendCommand(new AssetCommand("PickItem", group.ToArray()));
            EmitStep("warehouse-receive", "completed", "Components picked");

            if (_stopRequested)
                return;

            EmitStep("agv-to-assembly", "in-progress", "Transporting components to assembly");
            DateTime agvToAssemblyStartedAt = DateTime.UtcNow;
            await GetController("agv").SendCommand(new AssetCommand("MoveToStorageOperation", null));
            await GetController("agv").SendCommand(new AssetCommand("PickWarehouseOperation", _currentOrder.Items));
            await GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null));
            await GetController("agv").SendCommand(new AssetCommand("PutAssemblyOperation", null));

            TimeSpan agvToAssemblyElapsed = DateTime.UtcNow - agvToAssemblyStartedAt;
            if (agvToAssemblyElapsed < TimeSpan.FromSeconds(3))
                await Task.Delay(TimeSpan.FromSeconds(3) - agvToAssemblyElapsed);

            EmitStep("agv-to-assembly", "completed", "Components delivered to assembly");

            if (_stopRequested)
                return;

            EmitStep("assembly", "in-progress", "Assembly started");
            await GetController("assembly").SendCommand(new AssetCommand("start", null));
            EmitStep("assembly", "completed", "Assembly finished");

            if (_stopRequested)
                return;

            await GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null));

            EmitStep("agv-to-warehouse", "in-progress", "Picking assembled product and returning to warehouse");
            DateTime agvReturnStartedAt = DateTime.UtcNow;
            await GetController("agv").SendCommand(new AssetCommand("PickAssemblyOperation", _currentOrder.Items));
            await GetController("agv").SendCommand(new AssetCommand("MoveToStorageOperation", null));
            await GetController("agv").SendCommand(new AssetCommand("PutWarehouseOperation", null));

            TimeSpan agvReturnElapsed = DateTime.UtcNow - agvReturnStartedAt;
            if (agvReturnElapsed < TimeSpan.FromSeconds(3))
                await Task.Delay(TimeSpan.FromSeconds(3) - agvReturnElapsed);

            EmitStep("agv-to-warehouse", "completed", "Returned to warehouse");

            if (_stopRequested)
                return;

            EmitStep("warehouse-delivery", "in-progress", "Inserting finished product into warehouse");
            await InsertFinishedProduct();
            await Task.Delay(3000);
            EmitStep("warehouse-delivery", "completed", "Inserted into warehouse");

            if (_stopRequested)
                return;

            EmitStep("delivery", "in-progress", "Preparing outbound delivery");

            if (_stopRequested)
                return;

            await Task.Delay(1000);
            EmitStep("delivery", "completed", "Out for delivery");

            if (_stopRequested)
                return;
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
        if (_controllerRegistry.TryGetValue(assetName, out IAssetController? controller))
        {
            return controller;
        }
        else
        {
            IReadOnlyList<IAssetController> iAssetController = GetAssetControllers();
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
        IWarehouseController warehouse5 = GetWarehouseForTray(41);
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
        IEnumerable<IWarehouseController> warehouses = GetAssetControllers().OfType<IWarehouseController>().Where(w => w.MaxTray <= 40);
        foreach (IWarehouseController warehouse in warehouses)
            await warehouse.SendCommand(new AssetCommand("Refill", null));
    }

    public async Task PopulateWarehouses()
    {
        try
        {
            Item[] components = ServiceLocator.Instance.LocateAll<IPersistence>()[0].GetComponents();

            foreach (IGrouping<IWarehouseController, Item> group in components.GroupBy(c => GetWarehouseForTray(c.TrayId)))
                await group.Key.SendCommand(new AssetCommand("Populate", group.ToArray()));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PopulateWarehouses skipped: {ex.Message}");
        }
    }

    public QueueSnapshotDto GetQueueSnapshot()
    {
        QueueOrderSnapshotDto[] queuedOrders =
            OrderHandler.Instance.OrderQueue
            .Select(order => new QueueOrderSnapshotDto
                {
                    orderId = order.Id,
                    createdAt = DateTime.UtcNow,
                    status = "pending",
                    itemTrayIds = order.Items.Select(item => item.TrayId).ToArray()
                }).ToArray();


        QueueOrderSnapshotDto? currentOrder = _currentOrder == null
            ? null
            : new QueueOrderSnapshotDto
            {
                orderId = _currentOrder.Id,
                createdAt = DateTime.UtcNow,
                status = _state == ProductionState.executing 
                    ? "in-progress" 
                    : (_state == ProductionState.paused ? "paused" : "pending"),
                itemTrayIds = _currentOrder.Items.Select(item => item.TrayId).ToArray()
            };

        return new QueueSnapshotDto
        {
            currentOrder = currentOrder,
            queuedOrders = queuedOrders
        };
    }

    public MachineSnapshotDto[] GetMachinesSnapshot()
    {
        MachineSnapshotDto[] machines = _machineSnapshots.Values
            .OrderBy(machine => machine.Id)
            .Select(machine => new MachineSnapshotDto
            {
                id = machine.Id,
                name = machine.Name,
                type = machine.Type,
                connectionStatus = machine.ConnectionStatus,
                state = machine.State,
                currentTask = machine.CurrentTask,
                lastUpdatedAt = machine.LastUpdatedAt
            }).ToArray();
        return machines;
    }

    public OrderStatusSnapshotDto? GetOrderStatusSnapshot(int orderId)
    {
        if (_orderStatusByOrderId.TryGetValue(orderId, out OrderStatusSnapshot? snapshot))
        {
            return new OrderStatusSnapshotDto
            {
                orderId = snapshot.OrderId,
                stage = snapshot.Stage,
                state = snapshot.State,
                message = snapshot.Message,
                updatedAt = snapshot.UpdatedAt
            };   
        }

        if (_currentOrder != null && _currentOrder.Id == orderId)
        {
            string currentState = _state == ProductionState.executing
                ? "in-progress"
                : (_state == ProductionState.paused ? "error" : "pending");

            return new OrderStatusSnapshotDto
            {
                orderId = _currentOrder.Id,
                stage = "website",
                state = currentState,
                message = _state == ProductionState.executing
                    ? "Order accepted and waiting for first production step"
                    : (_state == ProductionState.paused
                        ? "Production paused"
                        : "Order accepted"),
                updatedAt = DateTime.UtcNow
            };
        }

        OrderDTO? queuedOrder = OrderHandler.Instance.OrderQueue.FirstOrDefault(order => order.Id == orderId);
        if (queuedOrder != null)
        {
            return new OrderStatusSnapshotDto
            {
                orderId = queuedOrder.Id,
                stage = "website",
                state = "pending",
                message = "Waiting in production queue",
                updatedAt = DateTime.UtcNow
            };
        }

        return null;
    }
    
    public Task StopProduction()
    {
        _stopRequested = true;
        _state = ProductionState.stopped;

        EmitStep("production", "error", "Production stopped by operator", "high");

        foreach (MachineSnapshot machine in _machineSnapshots.Values)
        {
            machine.State = "error";
            machine.CurrentTask = "Production stopped by operator";
            machine.LastUpdatedAt = DateTime.UtcNow;
        }

        Console.WriteLine("Production stopped by operator");

        return Task.CompletedTask;
    }

    public Task ResetProduction()
    {
        _stopRequested = false;
        _state = ProductionState.idle;
        _currentOrder = null;

        foreach (MachineSnapshot machine in _machineSnapshots.Values)
        {
            machine.State = "idle";
            machine.CurrentTask = "Waiting for production";
            machine.LastUpdatedAt = DateTime.UtcNow;
        }

        EmitStep("production", "reset", "Production reset by operator", "medium");

        Console.WriteLine("Production reset by operator");

        return Task.CompletedTask;
    }

    public void PluginStart()
    {

    }

    public void PluginDispose()
    {
        
    }
}
