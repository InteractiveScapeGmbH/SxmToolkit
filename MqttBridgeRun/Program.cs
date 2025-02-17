using CommandLine;
using Microsoft.Extensions.Logging;
using SxmMqttBridge;

namespace MqttBridgeRun;

class Program
{
    public class Options
    {
        [Option('i',"ip", HelpText = "Set the ip address of the scape x engine table.", Default = "10.0.0.20")]
        public string IpAddress { get; set; }
        [Option('l', "logLevel", HelpText="Set the minimum log level. (Default: Information)", Default = LogLevel.Information)]
        public LogLevel LogLevel { get; set; }
    }

    private static MqttBridge _bridge;
    static void Main(string[] args)
    {
        Console.CancelKeyPress += Shutdown;
        Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(option =>
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Information)
                    .AddFilter("System", LogLevel.Information)
                    .SetMinimumLevel(option.LogLevel)
                    .AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Program>();
            using (_bridge = new MqttBridge(logger, option.IpAddress))
            {
                while (true)
                {
                    if (!Console.KeyAvailable) continue;
                    var pressedKey = Console.ReadKey().Key;
                    if (pressedKey == ConsoleKey.Q) break;
                }
            }
        });

    }

    private static async void Shutdown(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        Console.WriteLine("Shutting down...");
        _bridge.Dispose();
        await Task.Delay(TimeSpan.FromSeconds(2));
        Environment.Exit(0);
    }
}
