namespace AGVController;
using System.Text.Json;
using System.Text;

public partial class AGVController
{
    private async Task<bool> Move(string programName)
    {
        var status = await ReadStatus();

        if (status == null)
            return false;

        if (status.State != 1)
            return false;
        
        await LoadProgramAsync(programName);
        await LoadExecuteStateAsync();
        return true;
    }

    private async Task<bool> WhileMoving()
    {
        try {
            while ((await ReadStatus()).State == 2) {
                Thread.Sleep(250);
            }
            return true;
        }
        catch (NullReferenceException ex) {
            return false;
        }
    }

    private async Task<StatusDTO?> ReadStatus()
    {
        var response = await httpClient.GetAsync($"{baseUrl}/status");
        StatusDTO? status = JsonSerializer.Deserialize<StatusDTO>(await response.Content.ReadAsStringAsync());

        return status;
    }

    private async Task LoadProgramAsync(string programName)
    {
        var payload = new Dictionary<string, string>
        {
            ["Program name"] = programName
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PutAsync($"{baseUrl}/status", content);
        response.EnsureSuccessStatusCode();
    }

    private async Task LoadExecuteStateAsync()
    {
        var payload = new Dictionary<string, string>
        {
            ["state"] = "2"
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PutAsync($"{baseUrl}/status", content);
        response.EnsureSuccessStatusCode();
    }
}