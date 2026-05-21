using AssemblyLineController;
using CommonAssetController;
using Warehouse1.Plugin;

namespace ProductionSystem.UnitTests;

public class AssetControllerTests
{
    // This unit test supports the user story about the Assembly Station using MQTT.
    // It verifies that the assembly controller rejects unsupported commands.
    //
    // This is important because the assembly station should only react to valid
    // production commands and should not continue with an unknown command.
    [Fact]
    public async Task AssemblyController_ReturnsFalse_ForUnsupportedCommand()
    {
        var controller = new AssemblyLineController();

        var command = new AssetCommand("invalid-command", null);

        var result = await controller.SendCommand(command);

        Assert.False(result);
    }

    // This unit test supports the user story about warehouse communication using SOAP.
    // It verifies that the warehouse controller can receive a command object.
    //
    // This test uses an unsupported command so it does not contact the real warehouse emulator.
    // That makes it useful as a safe unit test while still checking the controller command handling.
    [Fact]
    public async Task WarehouseController_ReturnsTrue_ForUnsupportedCommand()
    {
        var controller = new Warehouse1Controller();

        var command = new AssetCommand("invalid-command", null);

        var result = await controller.SendCommand(command);

        Assert.True(result);
    }
}