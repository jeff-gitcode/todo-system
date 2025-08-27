using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TodoSystem.Domain.Events;
using TodoSystem.Infrastructure.Configuration;
using TodoSystem.Infrastructure.Services;
using Xunit;

namespace TodoSystem.Infrastructure.Tests.Services;
    public class KafkaProducerServiceTests
{
    private readonly Mock<IProducer<string, string>> _mockProducer;
    private readonly Mock<ILogger<KafkaProducerService>> _mockLogger;
    private readonly KafkaConfig _kafkaConfig;
    private readonly ExternalTodoCreatedEvent _sampleEvent;

    public KafkaProducerServiceTests()
    {
        _mockProducer = new Mock<IProducer<string, string>>();
        _mockLogger = new Mock<ILogger<KafkaProducerService>>();
        _kafkaConfig = new KafkaConfig
        {
            BootstrapServers = "localhost:9092",
            ExternalTodoTopic = "test-topic",
            MessageTimeoutMs = 5000
        };
        _sampleEvent = new ExternalTodoCreatedEvent
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Test Todo",
            EventType = "TodoCreated",
            CorrelationId = Guid.NewGuid().ToString(),
            Source = "UnitTest"
        };
    }

    [Fact]
    public async Task PublishExternalTodoCreatedAsync_ShouldPublishMessage_WhenCalled()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<KafkaConfig>>();
        mockOptions.Setup(m => m.Value).Returns(_kafkaConfig);

        var deliveryResult = new DeliveryResult<string, string>
        {
            Message = new Message<string, string>
            {
                Key = _sampleEvent.Id,
                Value = JsonSerializer.Serialize(_sampleEvent)
            },
            TopicPartitionOffset = new TopicPartitionOffset(
                new TopicPartition(_kafkaConfig.ExternalTodoTopic, new Partition(1)),
                new Offset(100))
        };

        _mockProducer
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryResult);

        var service = CreateServiceWithMockedProducer(mockOptions.Object);

        // Act
        await service.PublishExternalTodoCreatedAsync(_sampleEvent);

        // Assert
        _mockProducer.Verify(
            p => p.ProduceAsync(
                _kafkaConfig.ExternalTodoTopic,
                It.Is<Message<string, string>>(m =>
                    m.Key == _sampleEvent.Id &&
                    m.Value == JsonSerializer.Serialize(_sampleEvent, (JsonSerializerOptions)null) &&
                    VerifyMessageHeaders(m.Headers)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockLogger.VerifyLogging(
            $"Successfully published event for todo {_sampleEvent.Id} to partition 1 at offset 100",
            LogLevel.Information,
            Times.Once());
    }

    [Fact]
    public async Task PublishExternalTodoCreatedAsync_ShouldThrowInvalidOperationException_WhenProduceExceptionOccurs()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<KafkaConfig>>();
        mockOptions.Setup(m => m.Value).Returns(_kafkaConfig);

        var kafkaError = new Error(ErrorCode.BrokerNotAvailable, "Broker not available");
        _mockProducer
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ProduceException<string, string>(kafkaError, null));

        var service = CreateServiceWithMockedProducer(mockOptions.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PublishExternalTodoCreatedAsync(_sampleEvent));

        Assert.Contains($"Failed to publish event for todo {_sampleEvent.Id}", exception.Message);
        _mockLogger.VerifyLogging(
            $"Failed to publish external todo created event for todo {_sampleEvent.Id}: Broker not available",
            LogLevel.Error,
            Times.Once());
    }

    [Fact]
    public async Task PublishExternalTodoCreatedAsync_ShouldThrowException_WhenGenericExceptionOccurs()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<KafkaConfig>>();
        mockOptions.Setup(m => m.Value).Returns(_kafkaConfig);

        _mockProducer
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var service = CreateServiceWithMockedProducer(mockOptions.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PublishExternalTodoCreatedAsync(_sampleEvent));

        _mockLogger.VerifyLogging(
            $"Unexpected error while publishing external todo created event for todo {_sampleEvent.Id}",
            LogLevel.Error,
            Times.Once());
    }

    [Fact]
    public void Dispose_ShouldFlushAndDisposeProducer_WhenCalled()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<KafkaConfig>>();
        mockOptions.Setup(m => m.Value).Returns(_kafkaConfig);

        var service = CreateServiceWithMockedProducer(mockOptions.Object);

        // Act
        service.Dispose();

        // Assert
        _mockProducer.Verify(p => p.Flush(It.IsAny<TimeSpan>()), Times.Once);
        _mockProducer.Verify(p => p.Dispose(), Times.Once);
        _mockLogger.VerifyLogging("Kafka producer disposed", LogLevel.Information, Times.Once());

        // Second dispose should not call flush or dispose again
        service.Dispose();
        _mockProducer.Verify(p => p.Flush(It.IsAny<TimeSpan>()), Times.Once);
        _mockProducer.Verify(p => p.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<KafkaConfig>>();
        mockOptions.Setup(m => m.Value).Returns(_kafkaConfig);

        _mockProducer.Setup(p => p.Flush(It.IsAny<TimeSpan>()))
            .Throws(new KafkaException(new Error(ErrorCode.Local_AllBrokersDown, "All brokers down")));

        var service = CreateServiceWithMockedProducer(mockOptions.Object);

        // Act
        service.Dispose();

        // Assert
        _mockProducer.Verify(p => p.Flush(It.IsAny<TimeSpan>()), Times.Once);
        _mockLogger.VerifyLogging("Error disposing Kafka producer", LogLevel.Error, Times.Once());
    }

    private KafkaProducerService CreateServiceWithMockedProducer(IOptions<KafkaConfig> options)
    {
        return new TestableKafkaProducerService(options, _mockLogger.Object, _mockProducer.Object);
    }

    private bool VerifyMessageHeaders(Headers headers)
    {
        return headers.TryGetLastBytes("eventType", out byte[] eventTypeBytes) &&
               Encoding.UTF8.GetString(eventTypeBytes) == _sampleEvent.EventType &&
               headers.TryGetLastBytes("correlationId", out byte[] correlationIdBytes) &&
               Encoding.UTF8.GetString(correlationIdBytes) == _sampleEvent.CorrelationId &&
               headers.TryGetLastBytes("source", out byte[] sourceBytes) &&
               Encoding.UTF8.GetString(sourceBytes) == _sampleEvent.Source;
    }

    // Testable version of KafkaProducerService that allows injecting a mock producer
    private class TestableKafkaProducerService : KafkaProducerService
    {
        public TestableKafkaProducerService(
            IOptions<KafkaConfig> kafkaConfig,
            ILogger<KafkaProducerService> logger,
            IProducer<string, string> producer)
            : base(kafkaConfig, logger)
        {
            // Use reflection to set the private _producer field
            var field = typeof(KafkaProducerService).GetField("_producer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(this, producer);
        }
    }
}

// Extension methods for verifying ILogger calls
public static class LoggerExtensions
{
    public static void VerifyLogging(this Mock<ILogger<KafkaProducerService>> logger, string expectedMessage, LogLevel expectedLogLevel, Times times)
    {
        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            times);
    }
}
