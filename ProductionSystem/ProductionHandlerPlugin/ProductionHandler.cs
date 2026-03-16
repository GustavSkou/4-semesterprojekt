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
            _controllerRegistry.Add(controller.GetAssetEnum(), controller);
        }
    }



    public void StartProduction()
    {
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
}
