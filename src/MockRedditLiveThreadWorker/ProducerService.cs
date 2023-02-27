using MassTransit;
using MessagingLibrary;
using Microsoft.Extensions.Options;

namespace MockRedditLiveThreadWorker
{
    public interface IProducerService
    {
        Task MessageQueue(RedditLiveThreadPayload message);
    }

    public class ProducerService : IProducerService
    {
        private readonly IBus _bus;

        private readonly Uri _jobQueueUri;

        public ProducerService(IOptions<MessageQueueServiceOptions> options, IBus bus)
        {
            _bus = bus;

            _jobQueueUri = new Uri(options.Value.QueueUri, "redditMessages");
        }

        /// <summary>
        /// Sends Message to MassTransit bus based on MessageQueueServiceOptions
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task MessageQueue(RedditLiveThreadPayload message)
        {
            var endPoint = await _bus.GetSendEndpoint(_jobQueueUri);

            await endPoint.Send(message);
        }
    }

    public class MessageQueueServiceOptions
    {
        public Uri QueueUri { get; set; }
    }
}
