using CommonProductionHandler;

namespace ProductionHandlerPlugin;

public class RequestHandler : IResumable, IStopable, IResetable, ICommandable
{
    public Task SendCommand(ProductionCommand command)
    {
        throw new NotImplementedException();
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
}