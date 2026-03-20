namespace WarehouseController;

using Common.Data;
using CommonAssetController;

public class WarehouseController : IAssetController
{
    public event EventHandler<ProductionEvent>? ProductionEventHandler;

    public string GetAssetName { get { return "warehouse"; } }

    public Task SendCommand(string command, string[] args)
    {
        throw new NotImplementedException();
    }

    public Task SendCommand(AssetCommand command)
    {
        throw new NotImplementedException();
    }

    Task<bool> IAssetController.Connect()
    {
        throw new NotImplementedException();
    }

    Task<bool> IAssetController.Disconnect()
    {
        throw new NotImplementedException();
    }

    Task<string> ReadStatus()
    {
        throw new NotImplementedException();
    }
}
