namespace ProductionDataHandlerPlugin;

using Common.Util;
using Common.Data;
using Common.Service;
using Common.ProductionDataSource;
using Common.Persistence;
using Common.MonitorDataSource;

public class ProductionDataHandler : IPlugin, IMonitorDataSource
{
    IPersistence persistenceService;

    public event EventHandler<string>? MonitorDataSource;

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

    public void PluginStart() {}

    public void PluginDispose() {}

    private void OnProductionEvent(object? obj, ProductionEvent e)
    {
        Console.WriteLine("ProductionDataHandler : ProductionEvent");
        MonitorDataSource.Invoke(this, e.Type);
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
