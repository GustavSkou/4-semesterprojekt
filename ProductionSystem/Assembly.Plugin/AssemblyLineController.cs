namespace AssemblyLineController;

using Common.Data;
using CommonAssetController;
using Microsoft.VisualBasic;
using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


public class AssemblyLineController : IAssetController
{
    private readonly IMqttClient mqttClient;
    private readonly MqttClientOptions mqttClientOptions;
    private readonly MqttClientFactory mqttFactory;

    private TaskCompletionSource<bool>? _healthCheckConfirmation;
    private readonly SemaphoreSlim _runGate = new(1, 1);
    private bool _isAssembling;
    private string _lastStatusPayload = string.Empty;
    private DateTime _lastStatusEventAt = DateTime.MinValue;

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
            ProductionEventHandler?.Invoke(this, new ProductionEvent
            {
                DateAndTime = DateTime.Now,
                Source = GetAssetName,
                Type = "connection",
                Level = "low",
                Description = $"Assembly MQTT client connected"
            });
            await SubscribeToTopics();
            return true;
        }
        catch (Exception ex)
        {
            ProductionEventHandler?.Invoke(this, new ProductionEvent
            {
                DateAndTime = DateTime.Now,
                Source = GetAssetName,
                Type = "error",
                Level = "high",
                Description = $"An error occurred while connecting to the MQTT broker: {ex.Message}"
            });
            return false;
        }
    }

    public async Task<bool> Disconnect()
    {
        try
        {
            await mqttClient.DisconnectAsync();
            ProductionEventHandler?.Invoke(this, new ProductionEvent {
                DateAndTime = DateTime.Now,
                Source = GetAssetName,
                Type = "disconnected",
                Level = "low",
                Description = $"Assembly MQTT client disconnected"
            });
            return true;
        }
        catch (Exception ex)
        {
            ProductionEventHandler?.Invoke(this, new ProductionEvent {
                DateAndTime = DateTime.Now,
                Source = GetAssetName,
                Type = "error",
                Level = "high",
                Description = $"An error occurred while disconnecting from the MQTT broker: {ex.Message}"
            });
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

    private void EmitStepStatus(string status, string description, string level = "low")
    {
        ProductionEventHandler?.Invoke(this, new ProductionEvent() {
            DateAndTime = DateTime.Now,
            Source = GetAssetName,
            Type = "step-status",
            Level = level,
            Description = $"{status}: {description}"
        });
    }

    private Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        var payload = e.ApplicationMessage.Payload;
        var payloadString = payload.IsEmpty
            ? string.Empty
            : Encoding.UTF8.GetString(payload.ToArray());

        if (e.ApplicationMessage.Topic == "emulator/status")
        {
            if (_isAssembling)
            {
                var now = DateTime.UtcNow;
                var isDuplicate = string.Equals(_lastStatusPayload, payloadString, StringComparison.Ordinal);
                var isThrottled = (now - _lastStatusEventAt) < TimeSpan.FromSeconds(2);

                if (!isDuplicate || !isThrottled)
                {
                    _lastStatusPayload = payloadString;
                    _lastStatusEventAt = now;
                    EmitStepStatus("in-progress", $"Assembly status: {payloadString}");
                }
            }
        } 
        else if (e.ApplicationMessage.Topic == "emulator/checkhealth") 
        {
            _healthCheckConfirmation?.TrySetResult(true);
        }

        return Task.CompletedTask;
    }

    public async Task<bool> SendCommand(AssetCommand command)
    {
        if (command.Name != "start")
            return false;

        await _runGate.WaitAsync();
        try 
        {
            _isAssembling = true;

            if (!mqttClient.IsConnected)
            {
                var connected = await Connect();
                if (!connected)
                    return false;
            }

            _healthCheckConfirmation = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var payloadDictionary = new Dictionary<string, string>
            {
                { "ProcessID", "123" }
            };

            string payload = JsonSerializer.Serialize(payloadDictionary);

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic("emulator/operation")
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            EmitStepStatus("in-progress", $"Assembly started {payload}");
            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            var confirmationTask = _healthCheckConfirmation.Task;
            var completedTask = await Task.WhenAny(
                confirmationTask,
                Task.Delay(TimeSpan.FromSeconds(60)));

            if (completedTask == confirmationTask)
            {
                EmitStepStatus("completed", $"Assembly finished");
                return true;
            }

            EmitStepStatus("error", $"Timed out waiting for assembly confirmation.", "high");
            return false;
        }
        finally
        {
            _isAssembling = false;
            _healthCheckConfirmation = null;
            _runGate.Release();
        }
    }
}