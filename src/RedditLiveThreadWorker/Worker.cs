using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MessagingLibrary;
using Serilog;
using ILogger = Serilog.ILogger;

namespace RedditLiveThreadWorker;

public class Worker : BackgroundService
{
    private readonly ILogger _logger = Log.ForContext<Worker>();
    private readonly string _url;
    private readonly IProducerService _producerService;

    public Worker(IProducerService producerService)
    {
        _url = Environment.GetEnvironmentVariable("REDDIT_WS") ?? throw new Exception("Missing Environment Variable");
        _producerService = producerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
            await ConnectWS(stoppingToken);
    }

    private async Task ConnectWS(CancellationToken stoppingToken)
    {
        try
        {
            using var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(_url), stoppingToken);

            var buf = new byte[1056];

            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buf, stoppingToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, stoppingToken);

                    _logger.Debug($"{result.CloseStatusDescription}");
                    return;
                }

                var message = Encoding.ASCII.GetString(buf, 0, result.Count);
                _logger.Debug($"{message}");

                var jsonNode = JsonSerializer.Deserialize<JsonNode>(message) ?? throw new Exception("Invalid Object.");

                if (!jsonNode["type"]!.GetValue<string>().Equals("update")) continue;

                var serializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
                var payloadDataJson = jsonNode["payload"]?["data"]?.ToJsonString() ??
                                      throw new Exception("Invalid Object.");
                var payloadData =
                    JsonSerializer.Deserialize<RedditLiveThreadPayload>(payloadDataJson, serializeOptions) ??
                    throw new Exception("Invalid Object.");

                await _producerService.MessageQueue(payloadData);
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Websocket Connection Failed.");
        }
    }
}
