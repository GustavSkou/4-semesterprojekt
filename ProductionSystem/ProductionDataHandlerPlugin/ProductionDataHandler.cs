namespace ProductionDataHandlerPlugin;

using Common.Util;
using Common.Data;
using Common.ProductionDataSource;
using Common.Presistence;
public class ProductionDataHandler
{
    public ProductionDataHandler()
    {
        GetProductionDataSources().First().EventHandler += OnProductionEvent;
    }

    private void OnProductionEvent(object? obj, ProductionEvent e)
    {

    }

    private IReadOnlyList<IProductionDataSource> GetProductionDataSources()
    {
        return ServiceLocator.Instance.LocateAll<IProductionDataSource>();
    }

    private IReadOnlyList<IPersistence> GetPersistenceServices()
    {
        return ServiceLocator.Instance.LocateAll<IPersistence>();
    }
}
