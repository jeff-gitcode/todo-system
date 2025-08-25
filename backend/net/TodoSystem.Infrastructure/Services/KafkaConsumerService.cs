using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using TodoSystem.Infrastructure.Configuration;

namespace TodoSystem.Infrastructure.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly KafkaConfig _kafkaConfig;
        private readonly ILogger<KafkaConsumerService> _logger;
        private IConsumer<string, string>? _consumer;

        public KafkaConsumerService(
            IOptions<KafkaConfig> kafkaConfig,
            ILogger<KafkaConsumerService> logger)
        {
            _kafkaConfig = kafkaConfig.Value;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaConfig.BootstrapServers,
                GroupId = _kafkaConfig.ConsumerGroupId ?? "todo-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, error) =>
                {
                    _logger.LogError("Kafka consumer error: {Error}", error);
                })
                .SetLogHandler((_, logMessage) =>
                {
                    _logger.LogDebug("Kafka consumer log: {Message}", logMessage.Message);
                })
                .Build();

            _consumer.Subscribe(_kafkaConfig.ExternalTodoTopic);

            _logger.LogInformation("Kafka consumer subscribed to topic: {Topic}", _kafkaConfig.ExternalTodoTopic);

            return Task.Run(() =>
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = _consumer.Consume(stoppingToken);
                            if (result != null)
                            {
                                _logger.LogInformation(
                                    "Consumed message: Key={Key}, Value={Value}, Partition={Partition}, Offset={Offset}",
                                    result.Message.Key,
                                    result.Message.Value,
                                    result.Partition.Value,
                                    result.Offset.Value);

                                // TODO: Deserialize and process the event as needed
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                }
                finally
                {
                    _consumer?.Close();
                    _logger.LogInformation("Kafka consumer closed");
                }
            }, stoppingToken);
        }

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}