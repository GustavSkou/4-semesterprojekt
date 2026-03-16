namespace ProductionDataHandlerPlugin;

using Common.Util;
using Common.ProductionDataSource;
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
}
