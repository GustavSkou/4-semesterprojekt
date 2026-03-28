namespace Warehouse2.Plugin;

public class Warehouse2Controller : WarehouseController.WarehouseController
{
    protected override string Url => "http://localhost:8082/Service.asmx";
    public override string GetAssetName => "warehouse2";

    public override int MinTray => 11;
    public override int MaxTray => 20;

    protected override bool ClearOnConnect => true;

}