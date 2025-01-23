namespace SxmMqttBridge;

public struct ScapeXMessage
{
    public string type;
    public string topic;
    public string value;

    public ScapeXMessage(string type, string topic, string value)
    {
        this.type = type;
        this.topic = topic;
        this.value = value;
    }
}