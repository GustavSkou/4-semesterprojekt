using System.Reflection;

namespace Common.Util;

public sealed class ServiceLocator
{
    public static ServiceLocator Instance { get; } = new ServiceLocator();

    private readonly List<Assembly> _pluginAssemblies = new();
    private readonly Dictionary<string, Assembly> _pluginRegistry = new();
    private readonly Dictionary<Type, List<object>> _serviceRegistry = new();

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
                if (!isCandidate(candidateType, serviceType))
                    continue;

                // Create the instance of the services 
                // Requires public parameterless constructor
                if (Activator.CreateInstance(candidateType) is T instance)
                {
                    services.Add(instance);
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
    private bool isCandidate(Type? candidateType, Type serviceType)
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
        foreach (var dll in Directory.EnumerateFiles(pluginsDir, "*.dll"))
        {
            var asm = Assembly.LoadFrom(dll);
            _pluginAssemblies.Add(asm);
            if (asm.FullName == null)
                continue;

            string asmName = asm.FullName.Split(',')[0];

            _pluginRegistry.TryAdd(asmName, asm);
            Console.WriteLine($"loaded: {asmName}");
        }
    }
}