using Common.Util;
using Common.Data;

using CommonAssetController;
using Common.ProductionDataSource;

namespace ProductionHandlerPlugin;

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
            //controller.Connect();
            _controllerRegistry.Add(controller.GetAssetName, controller);
        }
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
            StartProduction();
        }
    }

    private void OnProductionComplete(ProductionEvent e)
    {
        _state = ProductionState.idle;
        EventHandler?.Invoke(this, e); // raise event like this, eg notify data handler and pass Production event

        if (OrderHandler.Instance.OrderQueue.Count > 0)
        {
            _currentOrder = OrderHandler.Instance.OrderQueue.Dequeue();
            StartProduction();
        }
    }

    private async Task StartProduction()
    {
        Console.WriteLine($"Starting production! order: {_currentOrder.Id}");
        _state = ProductionState.executing;
        await GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null));
        await GetController("agv").SendCommand(new AssetCommand("MoveToStorageOperation", null));
        await GetController("agv").SendCommand(new AssetCommand("PickWarehouseOperation", _currentOrder.Items));
        await GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null));



        //await HandleProduction();
        //OnProductionComplete(new ProductionEvent());
    }

    private async Task<Task> HandleProduction()
    {
        await GetController("warehouse").SendCommand(new AssetCommand("pickitem", _currentOrder.Items));
        await GetController("agv").SendCommand(new AssetCommand("MoveToStorageOperation", null));

        await GetController("agv").SendCommand(new AssetCommand("PickWarehouseOperation", _currentOrder.Items));
        await GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null));
        await GetController("agv").SendCommand(new AssetCommand("PutAssemblyOperation", null));
        await GetController("assembly").SendCommand(new AssetCommand("start", null));
        await GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null));
        await GetController("agv").SendCommand(new AssetCommand("PickAssemblyOperation", _currentOrder.Items));
        await GetController("agv").SendCommand(new AssetCommand("MoveToStorageOperation", null));
        await GetController("agv").SendCommand(new AssetCommand("PutWarehouseOperation", null));
        await GetController("warehouse").SendCommand(new AssetCommand("InsertItem", new Item[0]));

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
}
