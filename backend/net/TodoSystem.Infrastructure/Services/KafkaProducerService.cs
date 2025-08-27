using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TodoSystem.Application.Services;
using TodoSystem.Domain.Events;
using TodoSystem.Infrastructure.Configuration;

namespace TodoSystem.Infrastructure.Services
{
    public class KafkaProducerService : IEventPublisher, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly KafkaConfig _kafkaConfig;
        private readonly ILogger<KafkaProducerService> _logger;
        private bool _disposed = false;

        public KafkaProducerService(
            IOptions<KafkaConfig> kafkaConfig,
            ILogger<KafkaProducerService> logger)
        {
            _kafkaConfig = kafkaConfig.Value;
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaConfig.BootstrapServers,
                MessageTimeoutMs = _kafkaConfig.MessageTimeoutMs,
                EnableIdempotence = true,
                Acks = Acks.All,
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 100
            };

            _producer = new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, error) =>
                {
                    _logger.LogError("Kafka producer error: {Error}", error);
                })
                .SetLogHandler((_, logMessage) =>
                {
                    _logger.LogDebug("Kafka producer log: {Message}", logMessage.Message);
                })
                .Build();

            _logger.LogInformation("Kafka producer initialized with servers: {BootstrapServers}",
                _kafkaConfig.BootstrapServers);
        }

        public async Task PublishExternalTodoCreatedAsync(ExternalTodoCreatedEvent eventData, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = eventData.Id,
                    Value = JsonSerializer.Serialize(eventData),
                    Headers = new Headers
                    {
                        { "eventType", System.Text.Encoding.UTF8.GetBytes(eventData.EventType) },
                        { "correlationId", System.Text.Encoding.UTF8.GetBytes(eventData.CorrelationId) },
                        { "source", System.Text.Encoding.UTF8.GetBytes(eventData.Source) }
                    }
                };

                _logger.LogInformation("Successfully publishing external todo created event for todo {TodoId} to topic {Topic}",
                    eventData.Id, _kafkaConfig.ExternalTodoTopic);

                _logger.LogInformation("Successfully publishing message: {Key}, {Message}", message.Key, message.Value);

                var deliveryReport = await _producer.ProduceAsync(_kafkaConfig.ExternalTodoTopic, message, cancellationToken);

                _logger.LogInformation("Successfully published event for todo {TodoId} to partition {Partition} at offset {Offset}",
                    eventData.Id, deliveryReport.Partition.Value, deliveryReport.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish external todo created event for todo {TodoId}: {Error}",
                    eventData.Id, ex.Error.Reason);
                throw new InvalidOperationException($"Failed to publish event for todo {eventData.Id}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while publishing external todo created event for todo {TodoId}",
                    eventData.Id);
                throw;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _producer?.Flush(TimeSpan.FromSeconds(10));
                    _producer?.Dispose();
                    _logger.LogInformation("Kafka producer disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing Kafka producer");
                }
                _disposed = true;
            }
        }
    }
}