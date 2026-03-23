namespace AGVController;

using System.Text.Json.Serialization;

internal class StatusDTO
{
	[JsonPropertyName("Battery")]
	public int Battery { get; set; }

	[JsonPropertyName("Program name")]
	public string ProgramName { get; set; } = string.Empty;

	[JsonPropertyName("State")]
	public int State { get; set; }

	[JsonPropertyName("TimeStamp")]
	public string TimeStamp { get; set; } = string.Empty;
}