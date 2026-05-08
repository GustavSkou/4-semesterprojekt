using Common.Data;
namespace Common.ProductionDataSource;

public interface IProductionDataSource
{
    public event EventHandler<ProductionEvent>? EventHandler;
}