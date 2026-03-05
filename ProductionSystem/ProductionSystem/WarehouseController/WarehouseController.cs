namespace WarehouseController;

using CommonAssetController;

public class WarehouseController : IAssetController
{
    Task<bool> IAssetController.Connect()
    {
        throw new NotImplementedException();
    }

    Task<bool> IAssetController.Disconnect()
    {
        throw new NotImplementedException();
    }

    Task<string> IAssetController.ReadStatus()
    {
        throw new NotImplementedException();
    }

    Task IAssetController.SendCommand(string command)
    {
        throw new NotImplementedException();
    }
}
