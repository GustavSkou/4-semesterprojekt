namespace CommonAssetController 
{
	public interface IAssetController
	{
		public Task<bool> Connect();
		public Task<bool> Disconnect();
		public Task SendCommand(AssetCommand command);
		public Task<string> ReadStatus();
		public AssetEnum GetAssetEnum();
	}
}