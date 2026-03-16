public class AssetCommand
{
    AssetEnum target;
    string name; 
    string payload;

    public AssetEnum Target { get { return target; } }
    public string Name { get { return name; } }
    public string Payload { get { return payload; } }

    public AssetCommand(AssetEnum target, string commandName, string payload)
    {
        this.target = target;
        this.name = commandName;
        this.payload = payload;
    }
}