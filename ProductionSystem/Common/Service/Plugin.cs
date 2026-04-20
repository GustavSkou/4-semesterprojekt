namespace Common.Service;

public interface IPlugin
{
    /// <summary>
    /// Run on plugin setup
    /// </summary>
    void PluginStart();

    /// <summary>
    /// Clean up on application shutdown
    /// </summary>
    void PluginDispose();
}