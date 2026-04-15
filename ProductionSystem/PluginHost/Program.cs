namespace ProductionSystem
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.Extensions.DependencyInjection;
    using Common.Util;
    using System;
    using Common.Persistence;
    using System.Data.Common;

    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceLocator = ServiceLocator.Instance;
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("http://localhost:5027");

            var mvcBuilder = builder.Services.AddControllers();
            foreach (var asm in serviceLocator.GetPluginAssemblies())
            {
                mvcBuilder.PartManager.ApplicationParts.Add(new AssemblyPart(asm));
            }
            Console.WriteLine($"Loaded {serviceLocator.GetPluginAssemblies().Count} plugin assemblies.");
            var app = builder.Build();
            
            app.MapControllers();
            
            var db = serviceLocator.LocateAll<IPersistence>();
            Console.WriteLine(db.First());
            
            /*
                var prodhandler = serviceLocator.LocateAll<IAssetController>();
                var controllers = serviceLocator.LocateAll<IAssetController>();
                Dictionary<string, IAssetController> controlReg = new Dictionary<string, IAssetController>();

                    foreach (var item in controllers)
                    {
                        controlReg.Add(item.GetAssetName, item);
                    }
                */
            //Console.WriteLine($"Loaded {controllers.Count} asset controllers.");
            

            //controlReg["agv"].SendCommand(new AssetCommand("test",null));

            app.Run();
        }
    }
}
