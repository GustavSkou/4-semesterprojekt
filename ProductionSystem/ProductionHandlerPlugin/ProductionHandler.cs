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
        _controllerRegistry = new Dictionary<AssetEnum, IAssetController>();
        foreach (IAssetController controller in getAssetControllers())
        {
            controller.Connect();
            _controllerRegistry.Add(controller.GetAssetEnum(), controller);
        }
    }

    public async Task StartProduction()
    {
        await getController(AssetEnum.warehouse).SendCommand(new AssetCommand("get", new Item[0]));
        await getController(AssetEnum.agv).SendCommand(new AssetCommand("warehouse", null));
        await getController(AssetEnum.agv).SendCommand(new AssetCommand("pick", new Item[0]));
        await getController(AssetEnum.agv).SendCommand(new AssetCommand("assembly", null));
        await getController(AssetEnum.agv).SendCommand(new AssetCommand("put", null));
        await getController(AssetEnum.assembly).SendCommand(new AssetCommand("start", null));
        await getController(AssetEnum.agv).SendCommand(new AssetCommand("assembly", null));
        await getController(AssetEnum.agv).SendCommand(new AssetCommand("pick", new Item[0]));
        await getController(AssetEnum.agv).SendCommand(new AssetCommand("warehouse", null));
        await getController(AssetEnum.agv).SendCommand(new AssetCommand("put", null));
        await getController(AssetEnum.warehouse).SendCommand(new AssetCommand("insert", new Item[0]));



        /*
        get items ready
        agv to warehouse
        agv pick items
        agv to assembly
        agv put items
        assembly start
        agv to assembly
        agv pick items
        agv to warehouse
        agv put items
        warehouse insert items
        */
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
    private IReadOnlyList<IAssetController> getAssetControllers()
    {
        return ServiceLocator.Instance.LocateAll<IAssetController>();
    }

    private IAssetController getController(AssetEnum assetEnum)
    {
        IAssetController controller;
        if (_controllerRegistry.TryGetValue(assetEnum, out controller))
        {
            return controller;
        }
        else
        {
            var iAssetController = getAssetControllers();
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
