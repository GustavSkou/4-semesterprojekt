using System.Reflection;

namespace Common.Util;

public sealed class ServiceLocator
{
    public static ServiceLocator Instance { get; } = new ServiceLocator();

    private readonly List<Assembly> _pluginAssemblies = new();
    private readonly Dictionary<string, Assembly> _pluginRegistry = new();
    private readonly Dictionary<Type, List<object>> _serviceCache = new();
    private readonly Dictionary<Type, object> _classInstances = new();

    private ServiceLocator() {
        string pluginsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Plugins"));
        if (!Directory.Exists(pluginsDir))
            return;
        
        ImportAssemblyPlugins(pluginsDir);
    }

    public IReadOnlyList<T> LocateAll<T>() where T : class
    {
        var serviceType = typeof(T);
        List<T> services = new List<T>();

        if (_serviceCache.TryGetValue(serviceType, out var cached))
            return cached.Cast<T>().ToList();

        foreach (var asm in _pluginAssemblies.Append(Assembly.GetExecutingAssembly())) {
            IEnumerable<Type> types;
            try {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex) {
                types = ex.Types.Where(t => t != null)!;
            }

            foreach (var candidateType in types) {
                if (!IsInstantiableAs(candidateType, serviceType))
                    continue;

                if (_classInstances.TryGetValue(candidateType, out var existing)) {
                    services.Add((T)existing);
                }
                else if (Activator.CreateInstance(candidateType) is T created) {
                    _classInstances[candidateType] = created;
                    services.Add(created);
                }
            }
        }
        _serviceCache[serviceType] = services.Cast<object>().ToList();
        return services;
    }

    public IReadOnlyList<Assembly> GetPluginAssemblies() {
        return _pluginAssemblies.AsReadOnly();
    }

    /// <summary>
    /// Check if "candidateType" is a candidate for being instanciated as Type "serviceType"
    /// </summary>
    /// <returns></returns>
    private bool IsInstantiableAs(Type? candidateType, Type serviceType)
    {
        if (candidateType is null || !candidateType.IsClass || candidateType.IsAbstract)
            return false;

        if (!serviceType.IsAssignableFrom(candidateType))
            return false;

        return true;
    }

    private void ImportAssemblyPlugins(string pluginsDir)
    {
        Console.WriteLine($"Loading assembly files");

        foreach (var dll in Directory.EnumerateFiles(pluginsDir, "*.Plugin.dll")) {
            Assembly asm;
            try {
                asm = Assembly.LoadFrom(dll);
            }
            catch (Exception) {
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