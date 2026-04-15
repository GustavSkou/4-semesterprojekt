using Common.Service;
using Common.Util;
using CommonProductionHandler;

namespace ProductionHandlerPlugin;

public class RequestHandler : IPlugin, IResumable, IStopable, IResetable, ICommandable
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
                Console.WriteLine("Order command");
                OrderHandler.Instance.AddOrderCommandToQueue(command);
                return;
            
            case "Refill":
                Console.WriteLine("Refill command");
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

    public void Start()
    {
        throw new NotImplementedException();
    }

    void IPlugin.Stop()
    {
        throw new NotImplementedException();
    }
}