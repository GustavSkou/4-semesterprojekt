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
        Console.WriteLine(this.Url);
    }
    
    [HttpPost("Command")]
    public IActionResult PostCommand([FromBody] ProductionCommand command)
    {
        if (command.Name == null)
            return BadRequest(command);
        
        foreach (var service in GetCommandableServices())
        {
            service.SendCommand(command);
        }

        Console.WriteLine(command.Name);

        return Ok(command);
    }

    /*
    
    {
        public required string Name { get; set; }
        public Dictionary<string,string>? Parameters {get; set;}
    
    }
    
    */

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