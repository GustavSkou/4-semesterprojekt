namespace Common.Data;

public class ProductionEvent
{
    public DateTime? DateAndTime { get; set; }
    public string? Description { get; set; }
    public string? Source { get; set; }
    public string? Type { get; set; }
    public string? Level { get; set; }

    public override string ToString()
    {
        return $"DateAndTime: {DateAndTime},\nDescription: {Description},\nSource: {Source},\nType: {Type},\nLevel: {Level}";
    }
}