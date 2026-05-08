namespace CommonProductionHandler;

public interface IResumable
{
    public Task Resume();
}

public interface IResetable
{
    public Task Reset();
}

public interface ICommandable
{
    public Task SendCommand(ProductionCommand command);
}

public interface IStopable
{
    public Task Stop();
}

public interface IProductionSnapshotSource
{
    QueueSnapshotDto GetQueueSnapshot();
    MachineSnapshotDto[] GetMachinesSnapshot();
    OrderStatusSnapshotDto? GetOrderStatusSnapshot(int orderId);
}