using Microsoft.Extensions.Logging;

namespace TodoSystem.Infrastructure.Services
{
    public interface IKafkaMessageService
    {
        void Execute(string message);
    }

    public class KafkaMessageService : IKafkaMessageService
    {
        private readonly ILogger<KafkaMessageService> _logger;

        public KafkaMessageService(ILogger<KafkaMessageService> logger)
        {
            _logger = logger;
        }

        public void Execute(string message)
        {
            _logger.LogInformation("Kafka message processed: {Message}", message);
        }
    }
}