using CommonAssetController;
using CommonProductionHandler;

namespace ProductionHandlerPlugin;

public class ProductionHandler :IProductionHandler, IStopable
{
    private Dictionary<AssetEnum, IAssetController> _controllerRegistry;

    public ProductionHandler()
    {
        _controllerRegistry = new Dictionary<AssetEnum, IAssetController>();
        foreach (IAssetController controller in getAssetControllers())
        {
            _controllerRegistry.Add(controller.GetAssetEnum(), controller);
        }
    }

    public Task Reset()
    {
        var controllers = getAssetControllers();        
        throw new NotImplementedException();
    }

    public Task Resume()
    {
        var controllers = getAssetControllers();
        throw new NotImplementedException();
    }

    public Task SendCommand(ProductionCommand command)
    {
        var controllers = getAssetControllers();
        throw new NotImplementedException();
    }

    public Task Stop()
    {
        var controllers = getAssetControllers();
        throw new NotImplementedException();
    }

    private void StartProduction()
    {
        
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
