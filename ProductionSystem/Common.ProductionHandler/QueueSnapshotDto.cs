namespace CommonProductionHandler;

public sealed class QueueSnapshotDto
{
    public QueueOrderSnapshotDto? currentOrder { get; init; }
    public QueueOrderSnapshotDto[] queuedOrders { get; init; } = Array.Empty<QueueOrderSnapshotDto>();
}