using Common.Util;
using Common.Data;

using CommonAssetController;
using Common.ProductionDataSource;
using CommonProductionHandler;

namespace ProductionHandlerPlugin;

public class ProductionHandler : IProductionDataSource, ICommandable
{
    private Dictionary<AssetEnum, IAssetController> _controllerRegistry;
    public event EventHandler<ProductionEvent> EventHandler; // raise event on this, to notify ProductionDataSource
    private Order _currentOrder;
    private ProductionState _state;

    public ProductionHandler()
    {
        OrderHandler.Instance.NewOrder += OnNewOrder;

        _controllerRegistry = new Dictionary<AssetEnum, IAssetController>();

        foreach (IAssetController controller in GetAssetControllers())
        {
            controller.Connect();
            _controllerRegistry.Add(controller.GetAssetEnum(), controller);
        }
    }

    /// <summary>
    /// When a new order is added to orderhandler's queue, this is invoked
    /// </summary>
    private void OnNewOrder(object sender, EventArgs e)
    {
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
        // raise event like this, eg notify data handler and pass Production event
        EventHandler?.Invoke(this, e);

        if (OrderHandler.Instance.OrderQueue.Count > 0)
        {
            _currentOrder = OrderHandler.Instance.OrderQueue.Dequeue();
            StartProduction();
        }
    }

    private async Task StartProduction()
    {
        _state = ProductionState.executing;
        await HandleProduction();
        OnProductionComplete(new ProductionEvent());
    }

    private async Task<Task> HandleProduction()
    {
        await GetController(AssetEnum.warehouse).SendCommand(new AssetCommand("pickitem", new Item[0]));
        await GetController(AssetEnum.agv).SendCommand(new AssetCommand("MoveToStorageOperation", null));

        await GetController(AssetEnum.agv).SendCommand(new AssetCommand("PickWarehouseOperation", new Item[0]));
        await GetController(AssetEnum.agv).SendCommand(new AssetCommand("MoveToAssemblyOperation", null));
        await GetController(AssetEnum.agv).SendCommand(new AssetCommand("PutAssemblyOperation", null));
        await GetController(AssetEnum.assembly).SendCommand(new AssetCommand("start", null));
        await GetController(AssetEnum.agv).SendCommand(new AssetCommand("MoveToAssemblyOperation", null));
        await GetController(AssetEnum.agv).SendCommand(new AssetCommand("PickAssemblyOperation", new Item[0]));
        await GetController(AssetEnum.agv).SendCommand(new AssetCommand("MoveToStorageOperation", null));
        await GetController(AssetEnum.agv).SendCommand(new AssetCommand("PutWarehouseOperation", null));
        await GetController(AssetEnum.warehouse).SendCommand(new AssetCommand("InsertItem", new Item[0]));

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

    private IAssetController GetController(AssetEnum assetEnum)
    {
        IAssetController controller;
        if (_controllerRegistry.TryGetValue(assetEnum, out controller))
        {
            return controller;
        }
        else
        {
            var iAssetController = GetAssetControllers();
            Dictionary<AssetEnum, IAssetController> controllerRegistry = new Dictionary<AssetEnum, IAssetController>();

            foreach (IAssetController c in iAssetController)
            {
                _controllerRegistry.TryAdd(c.GetAssetEnum(), c);
            }

            _controllerRegistry = controllerRegistry;

            if (!_controllerRegistry.TryGetValue(assetEnum, out controller))
                throw new Exception();

            return controller;
        }
    }

    public async Task SendCommand(ProductionCommand command)
    {
        if (command.Name == "GetItemsReady" && command.Parameters != null)
        {
            string components = command.Parameters["components"];
            string[] trayIds = components.Split(',');

            Item[] items = trayIds
                .Select(id => new Item { Id = int.Parse(id.Trim()) })
                .ToArray();

            await GetController(AssetEnum.warehouse)
                .SendCommand(new AssetCommand("PickItem", items));
        }
    }
}
