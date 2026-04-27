namespace AGVController;

using System.Text.Json;
using System.Text;
using System;
using System.Globalization;

public partial class AGVController
{
    private async Task<bool> ExecuteCommand(string programName)
    {
        var status = await ReadStatus();

        if (status == null)
            return false;

        if (status.state != 1)
            return false;

        if (programName != "MoveToChargerOperation")
        {
            if (status.battery < 20)
            {
                ProductionEventHandler?.Invoke(this, new Common.Data.ProductionEvent() {
                    DateAndTime = ConvertTimestampToDateTime(status.timeStamp),
                    Description = $"agv battery level is {status.battery}, moving to charging station",
                    Source = GetAssetName,
                    Type = "command",
                    Level = "low"
                });
                await MoveToCharger(new AssetCommand("MoveToChargerOperation", null));
            }
        }

        await LoadProgramAsync(programName);
        ExecuteCommandEvent(programName);
        return true;
    }

    private async Task<bool> WhileCharging()
    {
        StatusDTO? status;
        while (true)
        {
            try
            {
                status = await ReadStatus();
            }
            catch
            {
                return false;
            }

            if (status == null)
                return false;

            if (status.battery >= 90)
            {
                ProductionEventHandler?.Invoke(this, new Common.Data.ProductionEvent()
                {
                    DateAndTime = ConvertTimestampToDateTime(status.timeStamp),
                    Description = $"agv is charged to {status.battery}, returning to work",
                    Source = GetAssetName,
                    Type = "command",
                    Level = "low"
                });
                return true;
            }

            await Task.Delay(250);
        }
    }

    private async Task<bool> WhileDoing()
    {
        StatusDTO? status;
        while (true)
        {
            try
            {
                status = await ReadStatus();
                //Console.WriteLine(status.battery);
                // Console.WriteLine(status.state);
            }
            catch
            {
                return false;
            }

            if (status == null)
                return false;

            if (status.state == 1)
            {
                ProductionEventHandler?.Invoke(this, new Common.Data.ProductionEvent()
                {
                    DateAndTime = ConvertTimestampToDateTime(status.timeStamp),
                    Description = "agv is idle",
                    Source = GetAssetName,
                    Type = "command",
                    Level = "low"
                });
                return true;
            }

            if (status.state == 3)
                return false;

            if (status.battery < 10)
            {
                
            }

            await Task.Delay(250);
        }
    }

    private async Task<StatusDTO?> ReadStatus()
    {
        var response = await httpClient.GetAsync($"{baseUrl}/status");
        StatusDTO? status = JsonSerializer.Deserialize<StatusDTO>(await response.Content.ReadAsStringAsync());
        //Console.WriteLine(status.state.ToString());
        return status;
    }

    private async Task LoadProgramAsync(string programName)
    {
        var payload = new Dictionary<string, string>
        {
            ["Program name"] = programName,
            ["State"] = "1"
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PutAsync($"{baseUrl}/status", content);
        response.EnsureSuccessStatusCode();
        await LoadExecutingStateAsync();
    }
    private async Task LoadExecutingStateAsync()
    {
        var payload = new Dictionary<string, string>
        {
            ["State"] = "2"
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PutAsync($"{baseUrl}/status", content);
        response.EnsureSuccessStatusCode();
    }

    private static DateTime ConvertTimestampToDateTime(string timestamp)
    {
        if (long.TryParse(timestamp, out var unixValue))
        {
            if (timestamp.Length >= 13)
                return DateTimeOffset.FromUnixTimeMilliseconds(unixValue).UtcDateTime;

            return DateTimeOffset.FromUnixTimeSeconds(unixValue).UtcDateTime;
        }

        if (DateTime.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedDateTime))
            return parsedDateTime;

        return DateTime.UtcNow;
    }
}