using System.Diagnostics.Metrics;

namespace MessageConsumer.Services
{
    public interface IMetrics
    {
        Counter<int> MessagesCounter { get; }
        Histogram<float> MessageProcessingHistogram { get; }
    }

    public class Metrics : IMetrics
    {
        public Counter<int> MessagesCounter { get; }
        public Histogram<float> MessageProcessingHistogram { get; }
        
        public Metrics(string name)
        {
            var meter = new Meter(name);

            MessagesCounter = meter.CreateCounter<int>("messages-count");
            MessageProcessingHistogram = meter.CreateHistogram<float>("processing-duration", unit: "ms");
        }
    }
}
