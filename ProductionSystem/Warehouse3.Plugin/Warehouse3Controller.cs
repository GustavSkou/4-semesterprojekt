namespace Warehouse3.Plugin;

public class Warehouse3Controller : WarehouseController.WarehouseController
{
    protected override string Url => "http://localhost:8083/Service.asmx";
    public override string GetAssetName => "warehouse3";

    public override int MinTray => 21;
    public override int MaxTray => 30;
    protected override bool ClearOnConnect => true;

}