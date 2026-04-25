using Common.Data;
namespace Common.ProductionDataSource;

public interface IProductionDataSource
{
    public event EventHandler<ProductionEvent>? EventHandler;
}

public sealed class QueueOrderSnapshotDto
{
    public int orderId { get; init; }
    public DateTime createdAt { get; init; }
    public string status { get; init; } = "pending";
    public int[] itemTrayIds { get; init; } = Array.Empty<int>();
}

public sealed class QueueSnapshotDto
{
    public QueueOrderSnapshotDto? currentOrder { get; init; }
    public QueueOrderSnapshotDto[] queuedOrders { get; init; } = Array.Empty<QueueOrderSnapshotDto>();
}

public sealed class MachineSnapshotDto
{
    public string id {get; init; } = "";
    public string name { get; init; } = "";
    public string type { get; init; } = "";
    public string connectionStatus { get; init; } = "disconnected";
    public string state { get; init; } = "offline";
    public string currentTask { get; init; } = "Connection unavailable";
    public DateTime lastUpdatedAt { get; init; } = DateTime.UtcNow;
}

public sealed class OrderStatusSnapshotDto
{
    public int orderId { get; init; }
    public string stage { get; init; } = "website";
    public string state { get; init; } = "pending";
    public string message { get; init; } = "";
    public DateTime updatedAt { get; init; } = DateTime.UtcNow;
}

public interface IProductionSnapshotSource
{
    QueueSnapshotDto GetQueueSnapshot();
    MachineSnapshotDto[] GetMachinesSnapshot();
    OrderStatusSnapshotDto? GetOrderStatusSnapshot(int orderId);
}