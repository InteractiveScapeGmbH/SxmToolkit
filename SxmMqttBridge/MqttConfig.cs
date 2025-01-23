using System;
using System.Net;

namespace SxmMqttBridge;

/// <summary>
/// Holds the configuration about Room Id and Mqtt Server Url set in Touch and Object Assistant.
/// </summary>
public struct MqttConfig
{
    /// <summary>
    /// Topic to which the client should subscribe to.
    /// </summary>
    public string Topic
    {
        get => _topic;
        set
        {
            _topic = value;
            RoomId = _topic.Split('/')[0];
        }
    }

    private string _topic;
        
    public string RoomId { get; private set; }

    /// <summary>
    /// The Url of the used MQTT broker.
    /// </summary>
    public Uri MqttUrl { get; private set; }

    public void SetUrl(string urlString)
    {
        var url = (urlString.Contains("http://") || urlString.Contains("https://")) ? urlString : $"http://{urlString}";
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Port != 80)
        {
            MqttUrl = uri;
        }
        else
        {
            throw new ArgumentException($"The given Url ({urlString}) was invalid. It should have the form {{url}}:{{port}}");
        }
    }
}