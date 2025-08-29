using Microsoft.Extensions.Logging;

namespace TodoSystem.Infrastructure.Services
{
    public interface IKafkaMessageProcessor
    {
        void Execute(string message);
    }

    public class KafkaMessageProcessor : IKafkaMessageProcessor
    {
        private readonly ILogger<KafkaMessageProcessor> _logger;

        public KafkaMessageProcessor(ILogger<KafkaMessageProcessor> logger)
        {
            _logger = logger;
        }

        public void Execute(string message)
        {
            _logger.LogInformation("Kafka message processed: {Message}", message);
        }
    }
}