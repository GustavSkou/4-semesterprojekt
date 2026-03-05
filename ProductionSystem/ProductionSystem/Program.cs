namespace ProductionSystem
{
    using CommonAssetController;
    using AGVController;
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IAssetController agvController = new AGVController();

            string status = await agvController.ReadStatus();
            
            Console.WriteLine(status);
        }
    }
}
