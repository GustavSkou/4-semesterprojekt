namespace CommonProductionHandler;

public interface IProductionHandler
{
    public Task Resume();
    public Task Reset();
    public Task SendCommand(ProductionCommand command);
}