using SxmMqttBridge;

namespace TestMqttBridge;

public class TestMqttConfig
{
    private MqttConfig _config;
    [SetUp]
    public void Setup()
    {
        _config = new MqttConfig();
    }
    
    [Test]
    public void Test_Valid_Url_With_Port_And_Scheme()
    {
        _config.SetUrl("http://broker.hivemq.com:8884");
        Assert.AreEqual("broker.hivemq.com", _config.MqttUrl.Host);
        Assert.AreEqual(8884, _config.MqttUrl.Port);
    }
    
    [Test]
    public void Test_Valid_Url_With_Port_And_No_Scheme()
    {
        _config.SetUrl("broker.hivemq.com:8884");
        Assert.AreEqual("broker.hivemq.com", _config.MqttUrl.Host);
        Assert.AreEqual(8884, _config.MqttUrl.Port);
    }

    [Test]
    public void Test_Url_Without_Port()
    {
        Assert.Throws<ArgumentException>(delegate { _config.SetUrl("broker.hivemq.com"); });
    }
}