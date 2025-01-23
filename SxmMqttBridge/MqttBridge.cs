using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using TuioNet.Common;
using Newtonsoft.Json;

namespace SxmMqttBridge;

public class MqttBridge : IDisposable
{
    private const string DefaultScapeEngineAddress = "10.0.0.20";

    private const int ScapeEnginePort = 3330;
    private MqttConfig _config;
    private const string DefaultMqttUrl = "broker.hivemq.com";
    private const int DefaultPort = 8884;
    private readonly WebsocketClient _websocketClient;
    private readonly MqttClient _mqttClient;

    public event EventHandler<MqttConfig> OnConfigUpdate;
    private ILogger _logger;

    public MqttBridge(ILogger logger, string scapeEngineAddress = DefaultScapeEngineAddress)
    {
        _logger = logger;
        if (IPAddress.TryParse(scapeEngineAddress, out var ipAddress))
        {
            _websocketClient = new WebsocketClient(scapeEngineAddress, ScapeEnginePort, logger);
            _websocketClient.Connect();
            _mqttClient = new MqttClient(logger);
            _websocketClient.OnMessageReceived += ParseSxmConfig;
        }
        else
        {
            logger.LogError($"[MqttBridge] Invalid IP address of Scape X Engine host: {scapeEngineAddress}.");
        }
    }

    private void ParseSxmConfig(object? sender, MessageEventArgs messageArgs)
    {
        var message = Encoding.UTF8.GetString(messageArgs.Buffer, 0, messageArgs.Length);
        _logger.LogInformation($"[MqttBridge] Received from Scape X Engine: {message}");
        var sxmMessage = JsonConvert.DeserializeObject<ScapeXMessage>(message);
        switch (sxmMessage.type)
        {
            case "server":
                var url = string.IsNullOrEmpty(sxmMessage.value) ? $"{DefaultMqttUrl}:{DefaultPort}" : sxmMessage.value;
                try
                {
                    _config.SetUrl(url);
                }
                catch (ArgumentException exception)
                {
                    _config = new MqttConfig();
                    _logger.LogError($"[MqttBridge] {exception.Message}");
                }
                UpdateMqttSettings();
                break;
            case "subscribe":
                _config.Topic = sxmMessage.topic;
                UpdateMqttSettings();
                break;
        }
    }

    private void UpdateMqttSettings()
    {
        if (string.IsNullOrEmpty(_config.MqttUrl.Host) || string.IsNullOrEmpty(_config.Topic)) return;
        _mqttClient.UpdateServerSettings(_config.MqttUrl.Host, _config.MqttUrl.Port);
        _mqttClient.Subscribe($"sxm/{_config.Topic}");
        _mqttClient.Connect(OnMessage);
        OnConfigUpdate?.Invoke(this,_config);
    }

    private Task OnMessage(MqttApplicationMessageReceivedEventArgs message)
    {
        var payload = message.ApplicationMessage.PayloadSegment;
        var topic = message.ApplicationMessage.Topic.Replace("sxm/", "");
        if (payload.Array == null) return Task.CompletedTask;
        var decoded = Encoding.ASCII.GetString(payload.Array);
        var scapeMessage = JsonConvert.SerializeObject(new ScapeXMessage("message", topic, decoded));
        _websocketClient.Send(scapeMessage);

        return Task.CompletedTask;
    }

    private async void Disconnect()
    {
        _websocketClient.OnMessageReceived -= ParseSxmConfig;
        _websocketClient.Disconnect();
        if(_mqttClient.IsConnected)
            await _mqttClient.Disconnect();
    }

    public void Dispose()
    {
        Disconnect();
    }
}