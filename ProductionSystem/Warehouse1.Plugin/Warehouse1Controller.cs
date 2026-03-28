namespace Warehouse1.Plugin;

public class Warehouse1Controller : WarehouseController.WarehouseController
{
    protected override string Url => "http://localhost:8081/Service.asmx";
    public override string GetAssetName => "warehouse1";

    public override int MinTray => 1;
    public override int MaxTray => 10;

    protected override bool ClearOnConnect => true;

}