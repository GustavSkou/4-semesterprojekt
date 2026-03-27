using Common.Util;
using CommonProductionHandler;

namespace ProductionHandlerPlugin;

public class RequestHandler : IResumable, IStopable, IResetable, ICommandable
{
    public RequestHandler()
    {
        GetProductionHandler();
    }

    public async Task SendCommand(ProductionCommand command)
    {
        switch (command.Name)
        {
            case "order":
                Console.WriteLine("order command");
                OrderHandler.Instance.AddOrderCommandToQueue(command);
                return;
            
            case "refill":
                Console.WriteLine("refill command");
                await GetProductionHandler().RefillWarehouse();
                return;


            default:
                return;
        }
    }

    public Task Reset()
    {
        throw new NotImplementedException();
    }

    public Task Resume()
    {
        throw new NotImplementedException();
    }

    public Task Stop()
    {
        throw new NotImplementedException();
    }

    private ProductionHandler GetProductionHandler()
    {
        return ServiceLocator.Instance.LocateAll<ProductionHandler>()[0];
    }
}