namespace AssemblyLineController;

using Common.Data;
using CommonAssetController;
using Microsoft.VisualBasic;
using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


public class AssemblyLineController : IAssetController
{
    private readonly IMqttClient mqttClient;
    private readonly MqttClientOptions mqttClientOptions;
    private readonly MqttClientFactory mqttFactory;

    private TaskCompletionSource<bool>? _assemblyConfirmation;
    private TaskCompletionSource<bool>? _healthCheckConfirmation;

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
        try
        {
            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            Console.WriteLine("Assembly MQTT client connected.");
            await SubscribeToTopics();
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
            await mqttClient.DisconnectAsync();
            Console.WriteLine("Assembly MQTT client disconnected.");
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

    private async Task SubscribeToTopics()
    {
        var topicFilter = mqttFactory.CreateTopicFilterBuilder().WithTopic("emulator/status").WithAtLeastOnceQoS();

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(topicFilter)
            .WithTopicFilter(f => f.WithTopic("emulator/checkhealth").WithAtLeastOnceQoS())
            .Build();

        var response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        Console.WriteLine("MQTT client subscribed to topic.");

        Console.WriteLine(JsonSerializer.Serialize(response, SerializerOptions));
    }

    /*
        Handle update on subribe topics 
     */
    private Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        var payloadString = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);       

        if (e.ApplicationMessage.Topic == "emulator/status")
        {
            if (_assemblyConfirmation != null)
            {
                ProductionEventHandler?.Invoke(this, new ProductionEvent
                {
                    DateAndTime = DateTime.Now,
                    Source = GetAssetName,
                    Type = "status",
                    Level = "low",
                    Description = $"Assembly status: {payloadString}"
                });
            }
        } 
        else if (e.ApplicationMessage.Topic == "emulator/checkhealth") 
        {
            ProductionEventHandler?.Invoke(this, new ProductionEvent
            {
                DateAndTime = DateTime.Now,
                Source = GetAssetName,
                Type = "completion",
                Level = "low",
                Description = "Assembly finished"
            });
        
            _healthCheckConfirmation?.TrySetResult(true);
            _healthCheckConfirmation = null;
            _assemblyConfirmation = null;
        }

        return Task.CompletedTask;
    }

    public async Task<bool> SendCommand(AssetCommand command)
    {
        if (command.Name != "start")
            return false;

        if (!mqttClient.IsConnected)
        {
            var connected = await Connect();
            if (!connected)
                return false;
        }

        _assemblyConfirmation = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        _healthCheckConfirmation = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        Dictionary<string, string> payloadDictionary = new()
        {
            { "ProcessID", "123" }
        };

        string payload = JsonSerializer.Serialize(payloadDictionary);

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic("emulator/operation")
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        Console.WriteLine($"Sent MQTT message to emulator/operation: {payload}");

        var confirmationTask = _healthCheckConfirmation.Task;
        var completedTask = await Task.WhenAny(
            confirmationTask,
            Task.Delay(TimeSpan.FromSeconds(60)));

        if (completedTask == confirmationTask)
        {
            return true;
        }

        Console.WriteLine("Timed out waiting for assembly confirmation.");
        return false;
    }
}