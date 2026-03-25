using Common.Data;
namespace CommonAssetController;

public interface IAssetController
{
	public Task<bool> Connect();
	public Task<bool> Disconnect();
	public Task<bool> SendCommand(AssetCommand command);

	// To notify "ProductHandler" that new production data is avaliable, use ".invoke" on the eventhandler 
	public event EventHandler<ProductionEvent> ProductionEventHandler;
	public string GetAssetName { get; }
}