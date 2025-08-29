using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TodoSystem.Infrastructure.Configuration;
using TodoSystem.Infrastructure.Services;
using System;

namespace TodoSystem.Infrastructure.Tests.Services
{
    public class KafkaConsumerServiceTest
    {
        [Fact]
        public void ProcessMessage_ShouldCallExecute_OnKafkaMessageProcessor()
        {
            // Arrange
            var mockKafkaConfig = new Mock<IOptions<KafkaConfig>>();
            mockKafkaConfig.Setup(x => x.Value).Returns(new KafkaConfig { ExternalTodoTopic = "test-topic", BootstrapServers = "localhost:9092" });

            var mockLogger = new Mock<ILogger<KafkaConsumerService>>();
            var mockProcessor = new Mock<IKafkaMessageProcessor>();
            var mockScope = new Mock<IServiceScope>();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            mockServiceProvider.Setup(x => x.GetService(typeof(IKafkaMessageProcessor)))
                .Returns(mockProcessor.Object);
            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            var service = new KafkaConsumerService(mockKafkaConfig.Object, mockLogger.Object, mockScopeFactory.Object);

            // Act
            var testMessage = "{\"id\":\"123\",\"title\":\"Test Todo\"}";
            var processMessageMethod = typeof(KafkaConsumerService).GetMethod("ProcessMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processMessageMethod.Invoke(service, new object[] { testMessage });

            // Assert
            mockProcessor.Verify(x => x.Execute(testMessage), Times.Once);
        }

        [Fact]
        public void ProcessMessage_ShouldLogError_WhenProcessorThrowsException()
        {
            // Arrange
            var mockKafkaConfig = new Mock<IOptions<KafkaConfig>>();
            mockKafkaConfig.Setup(x => x.Value).Returns(new KafkaConfig { ExternalTodoTopic = "test-topic", BootstrapServers = "localhost:9092" });

            var mockLogger = new Mock<ILogger<KafkaConsumerService>>();
            var mockProcessor = new Mock<IKafkaMessageProcessor>();
            var mockScope = new Mock<IServiceScope>();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            var testException = new InvalidOperationException("Test exception");
            mockProcessor.Setup(x => x.Execute(It.IsAny<string>())).Throws(testException);

            mockServiceProvider.Setup(x => x.GetService(typeof(IKafkaMessageProcessor)))
                .Returns(mockProcessor.Object);
            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            var service = new KafkaConsumerService(mockKafkaConfig.Object, mockLogger.Object, mockScopeFactory.Object);

            // Act
            var testMessage = "{\"id\":\"123\",\"title\":\"Test Todo\"}";
            var processMessageMethod = typeof(KafkaConsumerService).GetMethod("ProcessMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processMessageMethod.Invoke(service, new object[] { testMessage });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error processing Kafka message")),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProcessMessage_ShouldLogError_WhenProcessorNotRegistered()
        {
            // Arrange
            var mockKafkaConfig = new Mock<IOptions<KafkaConfig>>();
            mockKafkaConfig.Setup(x => x.Value).Returns(new KafkaConfig { ExternalTodoTopic = "test-topic", BootstrapServers = "localhost:9092" });

            var mockLogger = new Mock<ILogger<KafkaConsumerService>>();
            var mockScope = new Mock<IServiceScope>();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Simulate missing IKafkaMessageProcessor registration
            mockServiceProvider.Setup(x => x.GetService(typeof(IKafkaMessageProcessor)))
                .Throws(new InvalidOperationException("Service not registered"));
            mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
            mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            var service = new KafkaConsumerService(mockKafkaConfig.Object, mockLogger.Object, mockScopeFactory.Object);

            // Act
            var testMessage = "{\"id\":\"123\",\"title\":\"Test Todo\"}";
            var processMessageMethod = typeof(KafkaConsumerService).GetMethod("ProcessMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processMessageMethod.Invoke(service, new object[] { testMessage });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error processing Kafka message")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}