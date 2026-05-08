namespace CommonProductionHandler;

public sealed class MachineSnapshotDto
{
    public string id { get; init; } = "";
    public string name { get; init; } = "";
    public string type { get; init; } = "";
    public string connectionStatus { get; init; } = "disconnected";
    public string state { get; init; } = "offline";
    public string currentTask { get; init; } = "Connection unavailable";
    public DateTime lastUpdatedAt { get; init; } = DateTime.UtcNow;
}