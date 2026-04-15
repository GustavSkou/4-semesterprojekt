namespace ProductionDataHandlerPlugin;

using Common.Util;
using Common.Data;
using Common.ProductionDataSource;
using Common.Persistence;
public class ProductionDataHandler
{
    IPersistence persistenceService;

    public ProductionDataHandler()
    {
        persistenceService = GetPersistenceServices()[0];

        // if there exists multipule prod data sources, they all invoke the onprodevent method
        foreach (var dataSource in GetProductionDataSources())
        {
            Console.WriteLine("setup datasource");
            dataSource.EventHandler += OnProductionEvent;
        }
    }

    private void OnProductionEvent(object? obj, ProductionEvent e)
    {
        Console.WriteLine("ProductionDataHandler : ProductionEvent");
        persistenceService.SaveProductionEvent(e);
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
