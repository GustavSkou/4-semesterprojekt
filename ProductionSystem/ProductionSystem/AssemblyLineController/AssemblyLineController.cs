namespace AssemblyLineController;

using CommonAssetController;
using MQTTnet;

public class AssemblyLineController : IAssetController
{
    private readonly string broker;
    private readonly int port;
    private readonly string topic;

    public AssemblyLineController()
    {
        broker = "localhost";
        port = 1883;

        var factory = new MqttFactory();

        // Create a MQTT client instance
        var mqttClient = factory.CreateMqttClient();

        // create MQTT client
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(broker, port)
            .WithCleanSession()
            .Build();
    }

    Task<bool> IAssetController.Connect()
    {
        throw new NotImplementedException();
    }

    Task<bool> IAssetController.Disconnect()
    {
        throw new NotImplementedException();
    }

    Task<string> IAssetController.ReadStatus()
    {
        throw new NotImplementedException();
    }

    Task IAssetController.SendCommand(string command)
    {
        throw new NotImplementedException();
    }
}
