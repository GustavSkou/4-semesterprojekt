namespace Warehouse5.Plugin;

public class Warehouse5Controller : WarehouseController.WarehouseController
{
    protected override string Url => "http://localhost:8085/Service.asmx";
    public override string GetAssetName => "warehouse5";

    public override int MinTray => 41;
    public override int MaxTray => 50;

    protected override bool ClearOnConnect => true;
}