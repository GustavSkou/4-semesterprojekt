namespace CommonProductionHandler;

public class ProductionCommand
{
    public required string Name { get; set; }
    public Dictionary<string, System.Text.Json.JsonElement>? Parameters { get; set; }
}