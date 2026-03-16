using Common.Data;

public class AssetCommand
{
    string name; 
    Item[]? items;

    public string Name { get { return name; } }
    public Item[]? Items { get { return items; } }

    public AssetCommand(string commandName, Item[] items)
    {
        this.name = commandName;
        this.items = items;
    }
}