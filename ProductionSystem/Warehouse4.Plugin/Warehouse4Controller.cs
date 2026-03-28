namespace Warehouse4.Plugin;

public class Warehouse4Controller : WarehouseController.WarehouseController
{
    protected override string Url => "http://localhost:8084/Service.asmx";
    public override string GetAssetName => "warehouse4";

    public override int MinTray => 31;
    public override int MaxTray => 40;
    protected override bool ClearOnConnect => true;

}