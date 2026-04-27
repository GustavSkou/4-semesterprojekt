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

    public Task Start()
    {
        return GetProductionHandler().StartProduction();
    }
    public Task Reset()
    {
        return GetProductionHandler().ResetProduction();
    }

    public Task Resume()
    {
        return GetProductionHandler().StartProduction();
    }

    public Task Stop()
    {
        return GetProductionHandler().StopProduction();
    }

    private ProductionHandler GetProductionHandler()
    {
        return ServiceLocator.Instance.LocateAll<ProductionHandler>()[0];
    }

    void IPlugin.PluginStart() {}

    void IPlugin.PluginDispose() {}
}