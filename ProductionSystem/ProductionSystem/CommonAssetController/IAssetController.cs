namespace CommonAssetController 
{
	public interface IAssetController
	{
		public Task<bool> Connect();
		public Task<bool> Disconnect();
		public Task SendCommand(string command);
		public Task<string> ReadStatus();
	}
}