namespace ProductionHandlerPlugin;


using Common.Util;
using Common.Data;

using CommonAssetController;
using Common.ProductionDataSource;
using Common.Presistence;


public class ProductionHandler : IProductionDataSource
{
    private Dictionary<string, IAssetController> _controllerRegistry;
    public event EventHandler<ProductionEvent>? EventHandler; // raise event on this, to notify ProductionDataSource
    private OrderDTO? _currentOrder = null;
    private ProductionState _state = ProductionState.idle;

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

    private void OnProductionEvent(object? sender, ProductionEvent e)
    {
        Console.WriteLine(e);
        //EventHandler?.Invoke(this, e);
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
        EventHandler?.Invoke(this, e); // raise event like this, eg notify data handler and pass Production event

        if (OrderHandler.Instance.OrderQueue.Count > 0)
        {
            _currentOrder = OrderHandler.Instance.OrderQueue.Dequeue();
            _ = StartProduction();
        }
    }
    private async Task StartProduction()
    {
        if (_currentOrder == null)
            return;

        Console.WriteLine($"Starting production! order: {_currentOrder.Id}");
        _state = ProductionState.executing;

        await HandleProduction();

        OnProductionComplete(new ProductionEvent
        {
            DateAndTime = DateTime.Now,
            Description = $"Order {_currentOrder.Id} completed",
            Source = "production-handler",
            Type = "completed",
            Level = "low"
        });
    }


    private async Task HandleProduction()
    {
        if (_currentOrder == null)
            return;

        foreach (var group in _currentOrder.Items.GroupBy(i => GetWarehouseForTray(i.TrayId)))
            await group.Key.SendCommand(new AssetCommand("PickItem", group.ToArray()));
        await GetController("agv").SendCommand(new AssetCommand("MoveToStorageOperation", null));
        await GetController("agv").SendCommand(new AssetCommand("PickWarehouseOperation", _currentOrder.Items));
        await GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null));
        await GetController("agv").SendCommand(new AssetCommand("PutAssemblyOperation", null));

        await GetController("assembly").SendCommand(new AssetCommand("start", null));

        await GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null));
        await GetController("agv").SendCommand(new AssetCommand("PickAssemblyOperation", _currentOrder.Items));
        await GetController("agv").SendCommand(new AssetCommand("MoveToStorageOperation", null));
        await GetController("agv").SendCommand(new AssetCommand("PutWarehouseOperation", null));
        await InsertFinishedProduct();
        return Task.CompletedTask;
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

}
