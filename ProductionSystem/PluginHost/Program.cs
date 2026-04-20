namespace ProductionSystem
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.Extensions.DependencyInjection;
    using Common.Util;
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceLocator = ServiceLocator.Instance;
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options => 
                options.AddDefaultPolicy(p => 
                    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
            builder.WebHost.UseUrls("http://localhost:5027");

            var mvcBuilder = builder.Services.AddControllers();
            foreach (var asm in serviceLocator.GetPluginAssemblies())
            {
                mvcBuilder.PartManager.ApplicationParts.Add(new AssemblyPart(asm));
            }

            var app = builder.Build();
            app.UseCors();
            app.MapControllers();
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
            Console.WriteLine($"Loaded {serviceLocator.GetPluginAssemblies().Count} plugin assemblies.");

            //controlReg["agv"].SendCommand(new AssetCommand("test",null));

            app.Run();
        }
    }
}
