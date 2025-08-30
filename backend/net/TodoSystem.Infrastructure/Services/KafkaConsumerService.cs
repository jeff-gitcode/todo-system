using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TodoSystem.Infrastructure.Configuration;

namespace TodoSystem.Infrastructure.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly KafkaConfig _kafkaConfig;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // Use this for accessing scoped services
        private IConsumer<string, string>? _consumer;
        private bool _disposed = false;

        public KafkaConsumerService(
            IOptions<KafkaConfig> kafkaConfig,
            ILogger<KafkaConsumerService> logger,
            IServiceScopeFactory scopeFactory) // Inject IServiceScopeFactory
        {
            _kafkaConfig = kafkaConfig.Value;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!await CheckIfTopicExistsAsync())
            {
                _logger.LogWarning("Kafka topic '{Topic}' does not exist. Consumer will not start.", _kafkaConfig.ExternalTodoTopic);
                return;
            }

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

            await Task.Run(() => ConsumeMessages(stoppingToken), stoppingToken);
        }

        protected virtual async Task<bool> CheckIfTopicExistsAsync()
        {
            try
            {
                using var adminClient = new AdminClientBuilder(
                    new AdminClientConfig { BootstrapServers = _kafkaConfig.BootstrapServers }
                ).Build();

                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                return metadata.Topics.Exists(t =>
                    t.Topic == _kafkaConfig.ExternalTodoTopic &&
                    t.Error.Code == ErrorCode.NoError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check Kafka topic existence");
                return false;
            }
        }

        private void ConsumeMessages(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer?.Consume(stoppingToken);
                        if (result != null)
                        {
                            _logger.LogInformation(
                                "Successfully consumed message: Key={Key}, Value={Value}, Partition={Partition}, Offset={Offset}",
                                result.Message.Key,
                                result.Message.Value,
                                result.Partition.Value,
                                result.Offset.Value);

                            // Process the message using a scoped context
                            ProcessMessage(result.Message.Value);
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
        }

        private void ProcessMessage(string message)
        {
            using var scope = _scopeFactory.CreateScope();
            try
            {
                var loggerService = scope.ServiceProvider.GetRequiredService<IKafkaMessageService>();
                loggerService.Execute(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _consumer?.Dispose(); // This is what should happen but isn't
                    _logger.LogInformation("Kafka consumer disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing Kafka consumer");
                }
                _disposed = true;
            }
        }
    }
}