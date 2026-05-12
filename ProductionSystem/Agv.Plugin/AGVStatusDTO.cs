namespace AGVController;

using System.Text.Json.Serialization;

internal class StatusDTO
{
	[JsonPropertyName("battery")]
	public int battery { get; set; }

	[JsonPropertyName("program name")]
	public string ProgramName { get; set; } = string.Empty;

	[JsonPropertyName("state")]
	public int state { get; set; }

	[JsonPropertyName("timeStamp")]
	public string timeStamp { get; set; } = string.Empty;

	public State? State
	{
		get { 
			if (state > 0 && state <= 3)
				return (State)state;
			else 
				return null;
		}
	}
}