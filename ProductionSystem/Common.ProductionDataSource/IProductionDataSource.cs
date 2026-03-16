namespace Common.ProductionDataSource;

public interface IProductionDataSource
{
    public event EventHandler<ProductionEvent> EventHandler;
}

public class ProductionEvent : EventArgs
{
    public ProductionEvent() { }
}
