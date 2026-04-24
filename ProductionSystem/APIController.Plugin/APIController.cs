using Microsoft.AspNetCore.Mvc;
using CommonProductionHandler;
using Common.Util;
using Common.Service;
using System.Reflection;

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
        object? snapshot = InvokeProductionHandler("GetQueueSnapshot");
        if (snapshot == null)
            return StatusCode(503, new { message = "Production handler unavailable" });

        return Ok(snapshot);
    }

    [HttpGet("Machines")]
    public ActionResult<object> GetMachinesSnapshot()
    {
        object? snapshot = InvokeProductionHandler("GetMachinesSnapshot");
        if (snapshot == null)
            return StatusCode(503, new { message = "Production handler unavailable" });

        return Ok(snapshot);
    }

    [HttpGet("OrderStatus/{orderId:int}")]
    public ActionResult<object> GetOrderStatusSnapshot(int orderId)
    {
        object? snapshot = InvokeProductionHandler("GetOrderStatusSnapshot", orderId);
        if (snapshot == null)
            return NotFound(new { message = "Order status not found" });

        return Ok(snapshot);
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

    private object? InvokeProductionHandler(string methodName, params object[] args)
    {
        IPlugin? handler = ServiceLocator.Instance
            .LocateAll<IPlugin>()
            .FirstOrDefault(o => string.Equals(o.GetType().Name, "ProductionHandler", StringComparison.Ordinal));

        if (handler == null)
            return null;

        MethodInfo? method = handler.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        if (method == null)
            return null;

        return method.Invoke(handler, args);
    }
}