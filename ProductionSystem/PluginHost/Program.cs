namespace ProductionSystem
{
    using Common.Util;
    using CommonAssetController;
    using CommonProductionHandler;

    internal class Program
    {
        static async Task Main(string[] args)
        {
            var controllers = ServiceLocator.Instance.LocateAll<IAssetController>();
            Console.WriteLine($"{controllers.Count}");
        }
    }
}
