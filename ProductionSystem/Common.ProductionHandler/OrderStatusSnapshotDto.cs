namespace CommonProductionHandler;

public sealed class OrderStatusSnapshotDto
{
    public int orderId { get; init; }
    public string stage { get; init; } = "website";
    public string state { get; init; } = "pending";
    public string message { get; init; } = "";
    public DateTime updatedAt { get; init; } = DateTime.UtcNow;
}