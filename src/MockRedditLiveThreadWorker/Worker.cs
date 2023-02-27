using System.Security.Cryptography;
using MessagingLibrary;

namespace MockRedditLiveThreadWorker;

public class Worker : BackgroundService
{
    private readonly IProducerService _producerService;

    public Worker(IProducerService producerService) => _producerService = producerService;

    /// <summary>
    /// Constantly produces RedditLiveThreadPayload messages until canceled.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var id = Guid.NewGuid();
            var payloadData = new RedditLiveThreadPayload
            {
                Author = $"User_{id}",
                Body = RandomNumberGenerator.GetInt32(1000000, 9999999).ToString(),
                Created = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                Id = id.ToString(),
                Name = $"LiveUpdate_{id}"
            };
            await _producerService.MessageQueue(payloadData);

            await RandomPause();
        }
    }

    /// <summary>
    /// creates a 5% chance to delay between 50-300ms
    /// </summary>
    /// <returns></returns>
    private static async Task RandomPause()
    {
        if (RandomNumberGenerator.GetInt32(1, 20) != 1) return;
        
        await Task.Delay(RandomNumberGenerator.GetInt32(5, 30) * 10);
    }
}