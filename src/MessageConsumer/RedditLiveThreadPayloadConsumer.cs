using MassTransit;
using MessageConsumer.Services;
using MessagingLibrary;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MessageConsumer
{
    public class RedditLiveThreadPayloadConsumer : IConsumer<RedditLiveThreadPayload>
    {
        private static readonly ILogger Logger = Log.ForContext<RedditLiveThreadPayloadConsumer>();
        private readonly IMetrics _metrics;

        public RedditLiveThreadPayloadConsumer(IMetrics metrics) => _metrics = metrics;

        /// <summary>
        /// Receives message from Mass Transit Bus for processing. Auto-ACK on complete
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Consume(ConsumeContext<RedditLiveThreadPayload> context)
        {
            var message = context.Message;
            
            CollectMetrics(message);
            
            Logger
                .ForContext("Command", message, destructureObjects: true)
                .Debug("Consumed: Id={Id}, Body={Body}", message.Id, message.Body);

            return Task.CompletedTask;
        }

        private void CollectMetrics(RedditLiveThreadPayload payload)
        {
            _metrics.MessagesCounter.Add(1);

            if (payload.Created != null)
                _metrics.MessageProcessingHistogram.Record(DateTime.UtcNow.Subtract(DateTimeOffset.FromUnixTimeSeconds((long)payload.Created).DateTime).Milliseconds);
        }
    }
}
