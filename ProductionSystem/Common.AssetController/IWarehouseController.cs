namespace CommonAssetController;

public interface IWarehouseController : IAssetController
{
    int MinTray { get; }
    int MaxTray { get; }
}