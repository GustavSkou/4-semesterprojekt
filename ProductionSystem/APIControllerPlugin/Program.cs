using Microsoft.AspNetCore.Mvc;
using CommonProductionHandler;
using Common.Util;
namespace APIController.Controllers;

[ApiController]
[Route("ProductionSystem")]
public class Controller : ControllerBase
{
    public Controller()
    {
        
    }
    
    [HttpPost("Command")]
    public IActionResult PostCommand([FromBody] ProductionCommand command)
    {
        GetCommandableServices();
        throw new NotImplementedException();
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