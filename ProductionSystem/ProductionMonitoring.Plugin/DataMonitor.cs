using Common.Service;
using Common.Util;
using CommonProductionHandler;
using Common.MonitorDataSource;

namespace MonitoringPlugin;

public class DataMonitor : IPlugin
{
    private readonly HashSet<string> errorSet = new(StringComparer.CurrentCultureIgnoreCase) { 
        "error", 
        "exception",
        "fail"
    };

    
    public void PluginStart()
    {
        foreach (IMonitorDataSource dataSource in GetDataSourceServices())
        {
            dataSource.MonitorDataSource += HandleEvents;
        }
    }
    
    public void PluginDispose() { }

    private void HandleEvents(object? sender, string e)
    {
        if (errorSet.Contains(e))
        {
            foreach (var service in GetStoppableServices())
            {
                service.Stop();
            }
        }
    }

    private IReadOnlyList<IStopable> GetStoppableServices()
    {
        return ServiceLocator.Instance.LocateAll<IStopable>();
    }

    private IReadOnlyList<IMonitorDataSource> GetDataSourceServices()
    {
        return ServiceLocator.Instance.LocateAll<IMonitorDataSource>();
    }
}