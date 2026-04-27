namespace ProductionHandlerPlugin;
using Common.Data;
public class ProductionSteps
{
    /// <summary>
    /// Return an array of tuples with maching emit and prod action steps
    /// </summary>
    /// <param name="EmitStep"></param>
    /// <param name="production"></param>
    /// <returns></returns>
    public static (Action emitStep, Func<Task<bool>> prodStep)[] GetSteps(Action<string,string,string,string> EmitStep, ProductionHandler production) {

        return new (Action, Func<Task<bool>>)[] 
        {
            (
                () => EmitStep("warehouse-receive", "in-progress", "Picking components from warehouses", "low"), 
                production.GetItemsReady
            ),
            (
                () => EmitStep("warehouse-receive", "completed", "Components picked", "low"), 
                () => Task.FromResult(true)
            ),
            (
                () => EmitStep("agv-to-assembly", "in-progress", "Transporting components to assembly", "low"),    
                () => production.GetController("agv").SendCommand(new AssetCommand("MoveToStorageOperation", null))
            ),
            (
                () => EmitStep("agv-to-assembly", "in-progress", "Transporting components to assembly", "low"),    
                () => production.GetController("agv").SendCommand(new AssetCommand("PickWarehouseOperation", production.CurrentOrder.Items))
            ),
            (
                () => EmitStep("agv-to-assembly", "in-progress", "Transporting components to assembly", "low"),    
                () => production.GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null))
            ),
            (
                () => EmitStep("agv-to-assembly", "in-progress", "Transporting components to assembly", "low"),    
                () => production.GetController("agv").SendCommand(new AssetCommand("PutAssemblyOperation", null))
            ),
            (
                () => EmitStep("agv-to-assembly", "completed", "Components delivered to assembly", "low"),         
                () => Task.FromResult(true)
            ),

            (
                () => EmitStep("assembly", "in-progress", "Assembly started", "low"),  
                () => production.GetController("assembly").SendCommand(new AssetCommand("start", null))),
            (
                () => EmitStep("assembly", "completed", "Assembly finished", "low"),   
                () => Task.FromResult(true)
            ),

            (
                () => EmitStep("agv-to-assembly", "in-progress", "AGV moving to warehouse", "low"),        
                () => production.GetController("agv").SendCommand(new AssetCommand("MoveToAssemblyOperation", null))
                ),
            (
                () => EmitStep("agv-to-assembly", "in-progress", "AGV picking assembled product", "low"),  
                () => production.GetController("agv").SendCommand(new AssetCommand("PickAssemblyOperation", new Item[] {new Item(){Name=$"pc-{production.CurrentOrder.Id}"}}))
                ),
            (
                () => EmitStep("agv-to-warehouse", "in-progress", "AGV returning to warehouse", "low"),    
                () => production.GetController("agv").SendCommand(new AssetCommand("MoveToStorageOperation", null))
            ),
            (
                () => EmitStep("warehouse-delivery", "in-progress", "AGV putting assembled production down at warehouse", "low"), 
                () => production.GetController("agv").SendCommand(new AssetCommand("PutWarehouseOperation", null))
            ),
            (
                () => EmitStep("warehouse-delivery", "in-progress", "Inserting finished product into warehouse", "low"), 
                production.InsertFinishedProduct),
            (
                () => EmitStep("warehouse-delivery", "completed", "Inserted into warehouse", "low"),              
                () => Task.FromResult(true)),
            
            (
                () => EmitStep("delivery", "in-progress", "Preparing outbound delivery", "low"), 
                () => {
                    Task.Delay(1000); 
                    return Task.FromResult(true);
                }
            ),
            (
                () => EmitStep("delivery", "completed", "Out for delivery", "low"), 
                () => Task.FromResult(true)
            )
        };   
    }
}