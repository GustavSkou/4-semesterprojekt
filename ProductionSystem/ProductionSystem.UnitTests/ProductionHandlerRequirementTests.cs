using System.Reflection;
using System.Text.Json;
using Common.Data;
using Common.Persistence;
using Common.PubSubDataSource;
using CommonAssetController;
using CommonProductionHandler;
using ProductionHandlerPlugin;
using Xunit;

namespace ProductionSystem.UnitTests;

public class ProductionHandlerRequirementTests : IDisposable
{
    private readonly List<ProductionHandler> _handlers = [];

    [Fact]
    public async Task F03_sends_configuration_to_production_assets()
    {
        ClearQueue();

        var warehouse1 = new FakeWarehouseController("warehouse1", 1, 10);
        var warehouse2 = new FakeWarehouseController("warehouse2", 11, 20);
        var warehouse3 = new FakeWarehouseController("warehouse3", 21, 30);
        var warehouse5 = new FakeWarehouseController("warehouse5", 41, 50);
        var agv = new FakeAssetController("agv");
        var assembly = new FakeAssetController("assembly");

        var handler = CreateHandler(
            [warehouse1, warehouse2, warehouse3, warehouse5, agv, assembly],
            subscribeToOrderHandler: true);

        OrderHandler.Instance.AddOrderCommandToQueue(CreateOrderCommand(301, [10, 11, 24]));

        await WaitUntilAsync(() => GetState(handler) == "idle");

        Assert.Contains(warehouse1.Commands, command =>
            command.Name == "PickItem" &&
            command.Items is not null &&
            command.Items.Select(item => item.TrayId).SequenceEqual([10]));
        Assert.Contains(warehouse2.Commands, command =>
            command.Name == "PickItem" &&
            command.Items is not null &&
            command.Items.Select(item => item.TrayId).SequenceEqual([11]));
        Assert.Contains(warehouse3.Commands, command =>
            command.Name == "PickItem" &&
            command.Items is not null &&
            command.Items.Select(item => item.TrayId).SequenceEqual([24]));
    }

    [Fact]
    public async Task F04_sends_soap_style_warehouse_commands_with_requested_components()
    {
        ClearQueue();

        var warehouse1 = new FakeWarehouseController("warehouse1", 1, 10);
        var warehouse2 = new FakeWarehouseController("warehouse2", 11, 20);
        var warehouse5 = new FakeWarehouseController("warehouse5", 41, 50);
        var handler = CreateHandler(
            [warehouse1, warehouse2, warehouse5, new FakeAssetController("agv"), new FakeAssetController("assembly")],
            subscribeToOrderHandler: true);

        OrderHandler.Instance.AddOrderCommandToQueue(CreateOrderCommand(401, [2, 12]));

        await WaitUntilAsync(() => GetState(handler) == "idle");

        Assert.Equal([2], warehouse1.Commands.Single(c => c.Name == "PickItem").Items!.Select(i => i.TrayId));
        Assert.Equal([12], warehouse2.Commands.Single(c => c.Name == "PickItem").Items!.Select(i => i.TrayId));
        Assert.Contains(warehouse5.Commands, command => command.Name == "CheckSpace");
        Assert.Contains(warehouse5.Commands, command => command.Name == "InsertItem");
    }

    [Fact]
    public async Task F05_sends_http_style_agv_commands_in_expected_sequence()
    {
        ClearQueue();

        var agv = new FakeAssetController("agv");
        var handler = CreateHandler(
            [
                new FakeWarehouseController("warehouse1", 1, 10),
                new FakeWarehouseController("warehouse5", 41, 50),
                agv,
                new FakeAssetController("assembly"),
            ],
            subscribeToOrderHandler: true);

        OrderHandler.Instance.AddOrderCommandToQueue(CreateOrderCommand(501, [1]));

        await WaitUntilAsync(() => GetState(handler) == "idle");

        Assert.Equal(
            [
                "MoveToStorageOperation",
                "PickWarehouseOperation",
                "MoveToAssemblyOperation",
                "PutAssemblyOperation",
                "MoveToAssemblyOperation",
                "PickAssemblyOperation",
                "MoveToStorageOperation",
                "PutWarehouseOperation",
            ],
            agv.Commands.Select(command => command.Name).ToArray());
    }

    [Fact]
    public async Task F06_sends_build_command_to_assembly_station()
    {
        ClearQueue();

        var assembly = new FakeAssetController("assembly");
        var handler = CreateHandler(
            [
                new FakeWarehouseController("warehouse1", 1, 10),
                new FakeWarehouseController("warehouse5", 41, 50),
                new FakeAssetController("agv"),
                assembly,
            ],
            subscribeToOrderHandler: true);

        OrderHandler.Instance.AddOrderCommandToQueue(CreateOrderCommand(601, [1]));

        await WaitUntilAsync(() => GetState(handler) == "idle");

        Assert.Equal(["start"], assembly.Commands.Select(command => command.Name).ToArray());
    }

    [Fact]
    public async Task F07_tracks_production_status_through_step_events()
    {
        ClearQueue();

        var events = new List<ProductionEvent>();
        var handler = CreateHandler(
            [
                new FakeWarehouseController("warehouse1", 1, 10),
                new FakeWarehouseController("warehouse5", 41, 50),
                new FakeAssetController("agv"),
                new FakeAssetController("assembly"),
            ],
            subscribeToOrderHandler: true);
        handler.EventHandler += (_, productionEvent) => events.Add(productionEvent);

        OrderHandler.Instance.AddOrderCommandToQueue(CreateOrderCommand(701, [1]));

        await WaitUntilAsync(() => GetState(handler) == "idle");

        var stepDescriptions = events
            .Where(productionEvent => string.Equals(productionEvent.Type, "step-status", StringComparison.OrdinalIgnoreCase))
            .Select(productionEvent => productionEvent.Description)
            .ToList();

        Assert.Contains("website|in-progress|Order 701 received", stepDescriptions);
        Assert.Contains("website|completed|Order 701 validated", stepDescriptions);
        Assert.Contains("warehouse-receive|completed|Components picked", stepDescriptions);
        Assert.Contains("agv-to-assembly|completed|Components delivered to assembly", stepDescriptions);
        Assert.Contains("assembly|completed|Assembly finished", stepDescriptions);
        Assert.Contains("warehouse-delivery|completed|Inserted into warehouse", stepDescriptions);
        Assert.Contains("delivery|completed|Out for delivery", stepDescriptions);
    }

    [Fact]
    public async Task F10_processes_orders_in_queue_order()
    {
        ClearQueue();

        var startedOrders = new List<int>();
        var handler = CreateHandler(
            [
                new FakeWarehouseController("warehouse1", 1, 10),
                new FakeWarehouseController("warehouse5", 41, 50),
                new FakeAssetController("agv"),
                new FakeAssetController("assembly"),
            ],
            subscribeToOrderHandler: false);
        handler.EventHandler += (_, productionEvent) =>
        {
            if (productionEvent.Description?.StartsWith("website|in-progress|Order ", StringComparison.Ordinal) == true)
            {
                var orderId = productionEvent.Description.Split(' ')[1];
                startedOrders.Add(int.Parse(orderId));
            }
        };

        OrderHandler.Instance.AddOrderCommandToQueue(CreateOrderCommand(801, [1]));
        OrderHandler.Instance.AddOrderCommandToQueue(CreateOrderCommand(802, [2]));
        InvokeOnNewOrder(handler);

        await WaitUntilAsync(() => startedOrders.Count == 2);

        Assert.Equal([801, 802], startedOrders);
    }

    [Fact]
    public async Task F11_starts_automatically_when_queue_is_not_empty()
    {
        ClearQueue();

        var warehouse1 = new FakeWarehouseController("warehouse1", 1, 10);
        var handler = CreateHandler(
            [
                warehouse1,
                new FakeWarehouseController("warehouse5", 41, 50),
                new FakeAssetController("agv"),
                new FakeAssetController("assembly"),
            ],
            subscribeToOrderHandler: false);

        OrderHandler.Instance.AddOrderCommandToQueue(CreateOrderCommand(901, [1]));
        InvokeOnNewOrder(handler);

        await WaitUntilAsync(() => warehouse1.Commands.Any(command => command.Name == "PickItem"));
        await WaitUntilAsync(() => GetState(handler) == "idle" && OrderHandler.Instance.OrderQueue.Count == 0);

        Assert.Empty(OrderHandler.Instance.OrderQueue);
    }

    [Fact]
    public async Task F13_stop_command_sets_production_state_to_stopped()
    {
        ClearQueue();

        var events = new List<ProductionEvent>();
        var handler = CreateHandler(
            [
                new FakeWarehouseController("warehouse1", 1, 10),
                new FakeWarehouseController("warehouse5", 41, 50),
                new FakeAssetController("agv"),
                new FakeAssetController("assembly"),
            ],
            subscribeToOrderHandler: false);
        handler.EventHandler += (_, productionEvent) => events.Add(productionEvent);

        await handler.Stop();

        Assert.Equal("paused", GetState(handler));
        Assert.Contains(events, productionEvent =>
            productionEvent.Type == "control" &&
            productionEvent.Description == "stop|Production stopped by operator");
    }

    [Fact]
    public async Task F14_reset_command_clears_production_state()
    {
        ClearQueue();

        var handler = CreateHandler(
            [
                new FakeWarehouseController("warehouse1", 1, 10),
                new FakeWarehouseController("warehouse5", 41, 50),
                new FakeAssetController("agv"),
                new FakeAssetController("assembly"),
            ],
            subscribeToOrderHandler: false);

        OrderHandler.Instance.AddOrderCommandToQueue(CreateOrderCommand(1001, [1]));
        SetPrivateField(handler, "_currentOrder", new OrderDTO(1001, [new Item { TrayId = 1 }]));
        SetState(handler, "paused");

        await handler.Reset();

        Assert.Equal("idle", GetState(handler));
        Assert.Null(GetCurrentOrder(handler));
        Assert.Equal(0, OrderHandler.Instance.OrderQueue.Count);
    }

    [Fact]
    public async Task F15_resume_command_continues_production()
    {
        ClearQueue();

        var warehouse1 = new FakeWarehouseController("warehouse1", 1, 10);
        var handler = CreateHandler(
            [
                warehouse1,
                new FakeWarehouseController("warehouse5", 41, 50),
                new FakeAssetController("agv"),
                new FakeAssetController("assembly"),
            ],
            subscribeToOrderHandler: false);

        OrderHandler.Instance.AddOrderCommandToQueue(CreateOrderCommand(1101, [1]));
        SetState(handler, "paused");

        await handler.Resume();
        await WaitUntilAsync(() => GetState(handler) == "idle" && OrderHandler.Instance.OrderQueue.Count == 0);

        Assert.Contains(warehouse1.Commands, command => command.Name == "PickItem");
    }

    [Fact]
    public async Task F16_lost_machine_communication_moves_system_to_safe_state()
    {
        ClearQueue();

        var events = new List<ProductionEvent>();
        var handler = CreateHandler(
            [
                new FakeWarehouseController("warehouse1", 1, 10),
                new FakeWarehouseController("warehouse5", 41, 50),
                new FakeAssetController("agv", commandNameToThrow: "MoveToStorageOperation"),
                new FakeAssetController("assembly"),
            ],
            subscribeToOrderHandler: false);
        handler.EventHandler += (_, productionEvent) => events.Add(productionEvent);

        OrderHandler.Instance.AddOrderCommandToQueue(CreateOrderCommand(1201, [1]));
        InvokeOnNewOrder(handler);

        await WaitUntilAsync(() => GetState(handler) == "paused");

        Assert.Contains(events, productionEvent =>
            string.Equals(productionEvent.Type, "step-status", StringComparison.OrdinalIgnoreCase) &&
            productionEvent.Description?.StartsWith("production|error|", StringComparison.Ordinal) == true);
    }

    [Fact(Skip = "F17 er ikke implementeret i production code: systemet gemmer ikke previous state og kan ikke revertere.")]
    public void F17_tracks_previous_state_and_can_revert()
    {
    }

    public void Dispose()
    {
        foreach (var handler in _handlers)
            handler.PluginDispose();

        ClearQueue();
    }

    private ProductionHandler CreateHandler(
        IEnumerable<IAssetController> assetControllers,
        bool subscribeToOrderHandler)
    {
        var handler = new ProductionHandler(
            assetControllers: assetControllers,
            persistenceServices: [new FakePersistence()],
            pubSubServices: [new FakePubSub()],
            delay: _ => Task.CompletedTask,
            subscribeToOrderHandler: subscribeToOrderHandler,
            populateWarehousesOnStart: false);

        _handlers.Add(handler);
        return handler;
    }

    private static ProductionCommand CreateOrderCommand(int id, int[] items)
    {
        return new ProductionCommand
        {
            Name = "order",
            Parameters = new Dictionary<string, JsonElement>
            {
                ["id"] = JsonSerializer.SerializeToElement(id),
                ["items"] = JsonSerializer.SerializeToElement(items),
            },
        };
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, int timeoutMs = 3000)
    {
        var startedAt = DateTime.UtcNow;
        while (!predicate())
        {
            if ((DateTime.UtcNow - startedAt).TotalMilliseconds > timeoutMs)
                throw new TimeoutException("Condition was not met before timeout.");

            await Task.Delay(10);
        }
    }

    private static string GetState(ProductionHandler handler)
    {
        var field = typeof(ProductionHandler).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return field.GetValue(handler)!.ToString()!;
    }

    private static void SetState(ProductionHandler handler, string stateName)
    {
        var field = typeof(ProductionHandler).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var enumValue = Enum.Parse(field.FieldType, stateName);
        field.SetValue(handler, enumValue);
    }

    private static OrderDTO? GetCurrentOrder(ProductionHandler handler)
    {
        var field = typeof(ProductionHandler).GetField("_currentOrder", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (OrderDTO?)field.GetValue(handler);
    }

    private static void SetPrivateField(ProductionHandler handler, string fieldName, object? value)
    {
        var field = typeof(ProductionHandler).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        field.SetValue(handler, value);
    }

    private static void ClearQueue()
    {
        OrderHandler.Instance.OrderQueue.Clear();
    }

    private static void InvokeOnNewOrder(ProductionHandler handler)
    {
        var method = typeof(ProductionHandler).GetMethod("OnNewOrder", BindingFlags.Instance | BindingFlags.NonPublic)!;
        method.Invoke(handler, [null, EventArgs.Empty]);
    }

    private sealed class FakeAssetController(string assetName, string? commandNameToThrow = null) : IAssetController
    {
        public List<AssetCommand> Commands { get; } = [];

        public event EventHandler<ProductionEvent>? ProductionEventHandler;

        public string GetAssetName => assetName;

        public Task<bool> Connect() => Task.FromResult(true);

        public Task<bool> Disconnect() => Task.FromResult(true);

        public Task<bool> SendCommand(AssetCommand command)
        {
            Commands.Add(command);

            if (string.Equals(command.Name, commandNameToThrow, StringComparison.Ordinal))
                throw new InvalidOperationException($"Simulated communication failure for {assetName}:{command.Name}");

            return Task.FromResult(true);
        }
    }

    private sealed class FakeWarehouseController(string assetName, int minTray, int maxTray) : IWarehouseController
    {
        public List<AssetCommand> Commands { get; } = [];

        public event EventHandler<ProductionEvent>? ProductionEventHandler;

        public string GetAssetName => assetName;
        public int MinTray => minTray;
        public int MaxTray => maxTray;

        public Task<bool> Connect() => Task.FromResult(true);

        public Task<bool> Disconnect() => Task.FromResult(true);

        public Task<bool> SendCommand(AssetCommand command)
        {
            Commands.Add(command);
            return Task.FromResult(true);
        }
    }

    private sealed class FakePersistence : IPersistence
    {
        public Item[] GetComponents() => [];

        public void SaveProductionEvent(ProductionEvent productionEvent)
        {
        }
    }

    private sealed class FakePubSub : IPubSubDataSource
    {
        public List<ProductionEvent> PublishedEvents { get; } = [];

        public void Publish(ProductionEvent e)
        {
            PublishedEvents.Add(e);
        }
    }
}
