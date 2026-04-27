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

    [HttpPost("Start")]
    public async Task<IActionResult> PostStart()
    {
        IReadOnlyList<IResumable> services = GetResumeableServices();

        if (services.Count == 0)
            return StatusCode(503, new { message = "No resumable production service found" });

        foreach (IResumable service in services)
        {
            await service.Resume();
        }

        return Ok(new { message = "Production started" });
    }

    [HttpPost("Resume")]
    public IActionResult PostResume()
    {
        GetResumeableServices();
        throw new NotImplementedException();
    }

    [HttpPost("Stop")]
    public async Task<IActionResult> PostStop()
    {
        IReadOnlyList<IStopable> services = GetStopableServices();
        if (services.Count == 0)
            return StatusCode(503, new { message = "No stopable production service found" });

        foreach (IStopable service in services)
        {
            await service.Stop();
        }

        return Ok(new { message = "Production stopped" });
    }

    [HttpPost("Reset")]
    public async Task<IActionResult> PostReset()
    {
        IReadOnlyList<IResetable> services = GetResetableServices();

        if (services.Count == 0)
            return StatusCode(503, new { message = "No resetable production service found" });

        foreach (IResetable service in services)
        {
            await service.Reset();
        }

        return Ok(new { message = "Production reset" });
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