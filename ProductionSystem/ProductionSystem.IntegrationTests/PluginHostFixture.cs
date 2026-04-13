using System.Diagnostics;
using System.Net;

namespace ProductionSystem.IntegrationTests;

public sealed class PluginHostFixture : IAsyncLifetime
{
    private Process? _pluginHostProcess;
    private readonly HttpClient _probeClient = new();

    public string BaseUrl { get; } = "http://localhost:5027";

    public async Task InitializeAsync()
    {
        if (await IsServerReadyAsync())
        {
            return;
        }

        var projectPath = GetPluginHostProjectPath();
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectPath}\" --no-build",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        _pluginHostProcess = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start PluginHost process.");

        if (!await WaitForServerReadyAsync())
        {
            var stdout = await _pluginHostProcess.StandardOutput.ReadToEndAsync();
            var stderr = await _pluginHostProcess.StandardError.ReadToEndAsync();
            throw new InvalidOperationException(
                $"PluginHost did not become ready in time.{Environment.NewLine}STDOUT:{Environment.NewLine}{stdout}{Environment.NewLine}STDERR:{Environment.NewLine}{stderr}");
        }
    }

    public Task DisposeAsync()
    {
        if (_pluginHostProcess is null)
        {
            return Task.CompletedTask;
        }

        if (!_pluginHostProcess.HasExited)
        {
            _pluginHostProcess.Kill(entireProcessTree: true);
            _pluginHostProcess.WaitForExit(5000);
        }

        _pluginHostProcess.Dispose();
        _pluginHostProcess = null;
        _probeClient.Dispose();
        return Task.CompletedTask;
    }

    public HttpClient CreateClient() => new() { BaseAddress = new Uri(BaseUrl) };

    private async Task<bool> WaitForServerReadyAsync()
    {
        for (var i = 0; i < 40; i++)
        {
            if (await IsServerReadyAsync())
            {
                return true;
            }

            if (_pluginHostProcess is { HasExited: true })
            {
                return false;
            }

            await Task.Delay(500);
        }

        return false;
    }

    private async Task<bool> IsServerReadyAsync()
    {
        try
        {
            var response = await _probeClient.GetAsync($"{BaseUrl}/ProductionSystem/TEST");
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    private static string GetPluginHostProjectPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "ProductionSystem", "PluginHost", "PluginHost.csproj");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException("Could not find ProductionSystem/PluginHost/PluginHost.csproj from test output path.");
    }
}

[CollectionDefinition("PluginHost integration collection", DisableParallelization = true)]
public sealed class PluginHostIntegrationCollection : ICollectionFixture<PluginHostFixture>
{
}