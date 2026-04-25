using Microsoft.AspNetCore.Mvc;
using CommonProductionHandler;
using Common.Util;
using Common.ProductionDataSource;

namespace APIController.Controllers;

[ApiController]
[Route("ProductionSystem")]
public class Controller : ControllerBase
{
    public Controller()
    {
        Console.WriteLine(this.Url);
    }

    [HttpPost("Command")]
    public IActionResult PostCommand([FromBody] ProductionCommand command)
    {
        Console.WriteLine($"command name {command.Name}");

        if (command.Name == null)
            return BadRequest(command);

        GetCommandableServices()[0].SendCommand(command);

        return Ok(command);
    }

    [HttpPost("Resume")]
    public IActionResult PostResume()
    {
        GetResumeableServices();
        throw new NotImplementedException();
    }

    [HttpPost("Stop")]
    public IActionResult PostStop()
    {
        GetStopableServices();
        throw new NotImplementedException();

    }

    [HttpPost("Reset")]
    public IActionResult PostReset()
    {
        GetResetableServices();
        throw new NotImplementedException();
    }

    [HttpGet("TEST")]
    public ActionResult<object> GetTest()
    {
        return "test";
    }

    [HttpGet("Queue")]
    public ActionResult<object> GetQueueSnapshot()
    {
        IProductionSnapshotSource? snapshotSource = GetSnapshotSource();
        if (snapshotSource == null)
            return StatusCode(503, new { message = "Production snapshot source unavailable" });

        QueueSnapshotDto snapshot = snapshotSource.GetQueueSnapshot();
        return Ok(snapshot);
    }

    [HttpGet("Machines")]
    public ActionResult<object> GetMachinesSnapshot()
    {
        IProductionSnapshotSource? snapshotSource = GetSnapshotSource();
        if (snapshotSource == null)
            return StatusCode(503, new { message = "Production snapshot source unavailable" });

        MachineSnapshotDto[] snapshot = snapshotSource.GetMachinesSnapshot();
        return Ok( new { machines = snapshot });
    }

    [HttpGet("OrderStatus/{orderId:int}")]
    public ActionResult<object> GetOrderStatusSnapshot(int orderId)
    {
        IProductionSnapshotSource? snapshotSource = GetSnapshotSource();
        if (snapshotSource == null)            
            return StatusCode(503, new { message = "Production snapshot source unavailable" });

        OrderStatusSnapshotDto? snapshot = snapshotSource.GetOrderStatusSnapshot(orderId);
        if (snapshot == null)
            return NotFound(new { message = "Order status not found" });

        return Ok(snapshot);
    }

    private IProductionSnapshotSource? GetSnapshotSource()
    {
        IReadOnlyList<IProductionSnapshotSource> sources = 
            ServiceLocator.Instance.LocateAll<IProductionSnapshotSource>();
        if (sources.Count == 0)
            return null;
        return sources[0];
    }

    private IReadOnlyList<ICommandable> GetCommandableServices()
    {
        return ServiceLocator.Instance.LocateAll<ICommandable>();
    }

    private IReadOnlyList<IResumable> GetResumeableServices()
    {
        return ServiceLocator.Instance.LocateAll<IResumable>();
    }

    private IReadOnlyList<IResetable> GetResetableServices()
    {
        return ServiceLocator.Instance.LocateAll<IResetable>();
    }

    private IReadOnlyList<IStopable> GetStopableServices()
    {
        return ServiceLocator.Instance.LocateAll<IStopable>();
    }
}