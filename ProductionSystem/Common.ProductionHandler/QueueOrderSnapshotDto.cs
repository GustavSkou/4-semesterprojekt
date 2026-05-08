namespace CommonProductionHandler;

public sealed class QueueOrderSnapshotDto
{
    public int orderId { get; init; }
    public DateTime createdAt { get; init; }
    public string status { get; init; } = "pending";
    public int[] itemTrayIds { get; init; } = Array.Empty<int>();
}