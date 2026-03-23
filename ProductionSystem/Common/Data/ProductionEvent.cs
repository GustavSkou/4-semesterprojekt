namespace Common.Data;

public class ProductionEvent
{
    public DateTime? DateAndTime { get; set; }
    public string? Description { get; set; }
    public string? Source { get; set; }
    public string? Type { get; set; }
    public string? Level { get; set; }
}