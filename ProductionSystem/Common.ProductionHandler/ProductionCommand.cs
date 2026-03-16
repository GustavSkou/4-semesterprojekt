namespace CommonProductionHandler;

public class ProductionCommand
{
    public required string Name { get; set; }
    public Dictionary<string,string>? Parameters {get; set;}
}