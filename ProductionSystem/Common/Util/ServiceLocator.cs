using System.Reflection;

namespace Common.Util;

public sealed class ServiceLocator
{
    public static ServiceLocator Instance { get; } = new ServiceLocator();

    private readonly List<Assembly> _pluginAssemblies = new();
    private readonly Dictionary<string, Assembly> _pluginRegistry = new();
    private readonly Dictionary<Type, List<object>> _serviceRegistry = new();
    private readonly Dictionary<Type, object> _serviceInstances = new();

    private ServiceLocator()
    {
        string pluginsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Plugins"));
        Console.WriteLine(pluginsDir);
        if (!Directory.Exists(pluginsDir))
        {
            Console.WriteLine("Could not find Plugins folder");
            return;
        }
        ImportAssemblyPlugins(pluginsDir);
    }

    //
    // Multipule call could be made to this still creating more than one instance of a given class
    //
    public IReadOnlyList<T> LocateAll<T>() where T : class
    {
        var serviceType = typeof(T);
        List<T> services = new List<T>();

        // check if the type is in the registry 
        if (_serviceRegistry.TryGetValue(serviceType, out var cached))
            return cached.Cast<T>().ToList();

        foreach (var asm in _pluginAssemblies.Append(Assembly.GetExecutingAssembly()))
        {
            IEnumerable<Type> types;
            try
            {
                types = asm.GetTypes(); // contains all types found in the assembly
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null)!;
            }

            foreach (var candidateType in types)
            {
                if (!IsCandidate(candidateType, serviceType))
                    continue;

                if (_serviceInstances.TryGetValue(candidateType, out var existing))
                {
                    services.Add((T)existing);
                }
                // Create the instance of the service
                // Requires public parameterless constructor
                else if (Activator.CreateInstance(candidateType) is T created)
                {
                    _serviceInstances[candidateType] = created;
                    services.Add(created);
                }
            }
        }
        _serviceRegistry[serviceType] = services.Cast<object>().ToList();
        return services;
    }

    public IReadOnlyList<Assembly> GetPluginAssemblies()
    {
        return _pluginAssemblies.AsReadOnly();
    }

    /// <summary>
    /// Check if "candidateType" is a candidate for being instanciated as Type "serviceType"
    /// </summary>
    /// <returns></returns>
    private bool IsCandidate(Type? candidateType, Type serviceType)
    {
        if (candidateType is null || candidateType.IsAbstract || candidateType.IsInterface)
            return false;

        if (!serviceType.IsAssignableFrom(candidateType))
            return false;

        return true;
    }

    /// <summary>
    /// Load all the assembly files into "_pluginAssemblies"
    /// </summary>
    /// <param name="pluginsDir"></param>
    private void ImportAssemblyPlugins(string pluginsDir)
    {
        Console.WriteLine($"Loading assembly files");
        foreach (var dll in Directory.EnumerateFiles(pluginsDir, "*.Plugin.dll"))
        {
            Assembly asm;
            try
            {
                asm = Assembly.LoadFrom(dll);
            }
            catch (Exception)
            {
                continue;
            }

            _pluginAssemblies.Add(asm);
            if (asm.FullName == null)
                continue;

            string asmName = asm.FullName.Split(',')[0];

            _pluginRegistry.TryAdd(asmName, asm);
            Console.WriteLine($"loaded: {asmName}");
        }
    }
}
