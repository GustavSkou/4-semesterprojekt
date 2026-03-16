using Common.Util;
using Common.Data;

using CommonAssetController;
using CommonProductionHandler;
using Common.ProductionDataSource;

namespace ProductionHandlerPlugin;

public class ProductionHandler : IProductionDataSource
{
    private Dictionary<AssetEnum, IAssetController> _controllerRegistry;

    public event EventHandler<ProductionEvent> EventHandler; // raise event on this, to notify ProductionDataSource

    public ProductionHandler()
    {
        OrderHandler.Instance.NewOrder += OnNewOrder();

        _controllerRegistry = new Dictionary<AssetEnum, IAssetController>();
        
        foreach (IAssetController controller in GetAssetControllers())
        {
            controller.Connect();
            _controllerRegistry.Add(controller.GetAssetEnum(), controller);
        }
    }

    /// <summary>
    /// When a new order is added to orderhandler, this is called
    /// </summary>
    private EventHandler OnNewOrder()
    {
        throw new NotImplementedException();
    }

    public async Task StartProduction()
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

        ProductionComplete(new ProductionEvent());
    }

    private void ProductionComplete(ProductionEvent e)
    {
        // raise event like this, eg notify data handler and pass Production event
        EventHandler.Invoke(this, e);
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
}
