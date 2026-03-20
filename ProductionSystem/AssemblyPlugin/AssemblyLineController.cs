namespace AssemblyLineController;

using Common.Data;
using CommonAssetController;
using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


public class AssemblyLineController : IAssetController
{
    private readonly IMqttClient mqttClient;
    private readonly MqttClientOptions mqttClientOptions;
    private readonly MqttClientFactory mqttFactory;
    static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public event EventHandler<ProductionEvent>? ProductionEventHandler;

    public string GetAssetName { get { return "assembly"; } }

    public AssemblyLineController()
    {
        mqttFactory = new MqttClientFactory();
        mqttClient = mqttFactory.CreateMqttClient();
        mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("localhost").Build();

        mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            return HandleReceivedMessage(e);
        };
    }

    public async Task<bool> Connect()
    {
        var mqttFactory = new MqttClientFactory();
        try
        {
            var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            Console.WriteLine("The MQTT client is connected.");
            SubribeToTopics();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while connecting to the MQTT broker: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> Disconnect()
    {
        try
        {
            await mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
            Console.WriteLine("The MQTT client is disconnected.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while disconnecting from the MQTT broker: {ex.Message}");
            return false;
        }
    }

    public Task<string> ReadStatus()
    {
        throw new NotImplementedException();
    }

    private async void SubribeToTopics()
    {
        var topicFilter = mqttFactory.CreateTopicFilterBuilder().WithTopic("emulator/status").WithAtLeastOnceQoS();

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder().WithTopicFilter(topicFilter).Build();

        var response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        Console.WriteLine("MQTT client subscribed to topic.");

        Console.WriteLine(JsonSerializer.Serialize(response, SerializerOptions));
    }

    /*
        Handle update on subribe topics 
     */
    private static Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        var payloadString = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

        Console.WriteLine("Received application message.");
        Console.WriteLine(JsonSerializer.Deserialize<JsonElement>(payloadString));
        return Task.CompletedTask;
    }

    public Task SendCommand(AssetCommand command)
    {
        Dictionary<string, string> payloadDictionary = new() { { "ProcessID", "123" } };

        string payload = JsonSerializer.Serialize(payloadDictionary);

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic("emulator/operation")
            .WithPayload(payload)
            .Build();
        return Task.CompletedTask;
    }
}