using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TodoSystem.Application.Dtos;
using TodoSystem.Infrastructure.ExternalServices;
using TodoSystem.Infrastructure.ExternalServices.Models;
using Xunit;

namespace TodoSystem.Infrastructure.Tests.Services
{
    public class JsonPlaceholderServiceTest
    {
        private readonly Mock<ILogger<JsonPlaceholderService>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly JsonPlaceholderService _service;
        private readonly CancellationToken _cancellationToken;

        public JsonPlaceholderServiceTest()
        {
            _mockLogger = new Mock<ILogger<JsonPlaceholderService>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://jsonplaceholder.typicode.com/")
            };
            _service = new JsonPlaceholderService(_httpClient, _mockLogger.Object);
            _cancellationToken = CancellationToken.None;
        }

        #region GetTodosAsync Tests

        [Fact]
        public async Task GetTodosAsync_Should_Return_Todos_When_Successful()
        {
            // Arrange
            var externalTodos = new[]
            {
                new JsonPlaceholderTodo { Id = 1, Title = "Todo 1", Completed = false },
                new JsonPlaceholderTodo { Id = 2, Title = "Todo 2", Completed = true }
            };

            var jsonResponse = JsonSerializer.Serialize(externalTodos);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            SetupHttpMessageHandler("todos", HttpMethod.Get, httpResponse);

            // Act
            var result = await _service.GetTodosAsync(_cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            var todoList = result.ToList();
            Assert.Equal("Todo 1", todoList[0].Title);
            Assert.Equal("Todo 2", todoList[1].Title);

            VerifyLoggerCalled(LogLevel.Information, "Fetching todos from JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Information, "Successfully fetched 2 todos from JSONPlaceholder");
        }

        [Fact]
        public async Task GetTodosAsync_Should_Return_Empty_Collection_When_No_Todos()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };

            SetupHttpMessageHandler("todos", HttpMethod.Get, httpResponse);

            // Act
            var result = await _service.GetTodosAsync(_cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            VerifyLoggerCalled(LogLevel.Information, "Fetching todos from JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Warning, "No todos received from JSONPlaceholder");
        }

        [Fact]
        public async Task GetTodosAsync_Should_Throw_InvalidOperationException_On_HttpRequestException()
        {
            // Arrange
            SetupHttpMessageHandlerToThrow("todos", HttpMethod.Get, new HttpRequestException("Network error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetTodosAsync(_cancellationToken));

            Assert.Equal("Failed to fetch todos from external service", exception.Message);
            Assert.IsType<HttpRequestException>(exception.InnerException);

            VerifyLoggerCalled(LogLevel.Information, "Fetching todos from JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Error, "HTTP error occurred while fetching todos from JSONPlaceholder");
        }

        [Fact]
        public async Task GetTodosAsync_Should_Rethrow_General_Exception()
        {
            // Arrange
            SetupHttpMessageHandlerToThrow("todos", HttpMethod.Get, new ArgumentException("Invalid argument"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetTodosAsync(_cancellationToken));

            Assert.Equal("Invalid argument", exception.Message);

            VerifyLoggerCalled(LogLevel.Information, "Fetching todos from JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Error, "Unexpected error occurred while fetching todos from JSONPlaceholder");
        }

        #endregion

        #region GetTodoByIdAsync Tests

        [Fact]
        public async Task GetTodoByIdAsync_Should_Return_Todo_When_Found()
        {
            // Arrange
            var todoId = 1;
            var externalTodo = new JsonPlaceholderTodo { Id = todoId, Title = "Test Todo", Completed = false };
            var jsonResponse = JsonSerializer.Serialize(externalTodo);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            SetupHttpMessageHandler($"todos/{todoId}", HttpMethod.Get, httpResponse);

            // Act
            var result = await _service.GetTodoByIdAsync(todoId, _cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Todo", result.Title);

            VerifyLoggerCalled(LogLevel.Information, $"Fetching todo {todoId} from JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Information, $"Successfully fetched todo {todoId} from JSONPlaceholder");
        }

        [Fact]
        public async Task GetTodoByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            var todoId = 999;
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };

            SetupHttpMessageHandler($"todos/{todoId}", HttpMethod.Get, httpResponse);

            // Act
            var result = await _service.GetTodoByIdAsync(todoId, _cancellationToken);

            // Assert
            Assert.Null(result);

            VerifyLoggerCalled(LogLevel.Information, $"Fetching todo {todoId} from JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Warning, $"Todo {todoId} not found in JSONPlaceholder");
        }

        [Fact]
        public async Task GetTodoByIdAsync_Should_Return_Null_On_404_HttpRequestException()
        {
            // Arrange
            var todoId = 999;
            SetupHttpMessageHandlerToThrow($"todos/{todoId}", HttpMethod.Get,
                new HttpRequestException("Response status code does not indicate success: 404 (Not Found)."));

            // Act
            var result = await _service.GetTodoByIdAsync(todoId, _cancellationToken);

            // Assert
            Assert.Null(result);

            VerifyLoggerCalled(LogLevel.Information, $"Fetching todo {todoId} from JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Warning, $"Todo {todoId} not found in JSONPlaceholder");
        }

        [Fact]
        public async Task GetTodoByIdAsync_Should_Throw_InvalidOperationException_On_Non404_HttpRequestException()
        {
            // Arrange
            var todoId = 1;
            SetupHttpMessageHandlerToThrow($"todos/{todoId}", HttpMethod.Get,
                new HttpRequestException("Network error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetTodoByIdAsync(todoId, _cancellationToken));

            Assert.Equal($"Failed to fetch todo {todoId} from external service", exception.Message);
            Assert.IsType<HttpRequestException>(exception.InnerException);

            VerifyLoggerCalled(LogLevel.Information, $"Fetching todo {todoId} from JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Error, $"HTTP error occurred while fetching todo {todoId} from JSONPlaceholder");
        }

        #endregion

        #region CreateTodoAsync Tests

        [Fact]
        public async Task CreateTodoAsync_Should_Return_True_When_Successful()
        {
            // Arrange
            var todoDto = new TodoDto { Id = Guid.NewGuid().ToString(), Title = "New Todo" };
            var httpResponse = new HttpResponseMessage(HttpStatusCode.Created);

            SetupHttpMessageHandler("todos", HttpMethod.Post, httpResponse);

            // Act
            var result = await _service.CreateTodoAsync(todoDto, _cancellationToken);

            // Assert
            Assert.True(result);

            VerifyLoggerCalled(LogLevel.Information, "Adding new todo to JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Information, "Successfully added new todo to JSONPlaceholder");
        }

        [Fact]
        public async Task CreateTodoAsync_Should_Return_False_When_Request_Fails()
        {
            // Arrange
            var todoDto = new TodoDto { Id = Guid.NewGuid().ToString(), Title = "New Todo" };
            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            SetupHttpMessageHandler("todos", HttpMethod.Post, httpResponse);

            // Act
            var result = await _service.CreateTodoAsync(todoDto, _cancellationToken);

            // Assert
            Assert.False(result);

            VerifyLoggerCalled(LogLevel.Information, "Adding new todo to JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Error, "Failed to add new todo to JSONPlaceholder. Status code: BadRequest");
        }

        [Fact]
        public async Task CreateTodoAsync_Should_Throw_InvalidOperationException_On_HttpRequestException()
        {
            // Arrange
            var todoDto = new TodoDto { Id = Guid.NewGuid().ToString(), Title = "New Todo" };
            SetupHttpMessageHandlerToThrow("todos", HttpMethod.Post, new HttpRequestException("Network error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateTodoAsync(todoDto, _cancellationToken));

            Assert.Equal("Failed to add todo to external service", exception.Message);
            Assert.IsType<HttpRequestException>(exception.InnerException);

            VerifyLoggerCalled(LogLevel.Information, "Adding new todo to JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Error, "HTTP error occurred while adding new todo to JSONPlaceholder");
        }

        #endregion

        #region UpdateTodoAsync Tests

        [Fact]
        public async Task UpdateTodoAsync_Should_Return_True_When_Successful()
        {
            // Arrange
            var todoDto = new TodoDto { Id = "123", Title = "Updated Todo" };
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

            SetupHttpMessageHandler("todos/123", HttpMethod.Put, httpResponse);

            // Act
            var result = await _service.UpdateTodoAsync(todoDto, _cancellationToken);

            // Assert
            Assert.True(result);

            VerifyLoggerCalled(LogLevel.Information, "Updating todo 123 in JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Information, "Successfully updated todo 123 in JSONPlaceholder");
        }

        [Fact]
        public async Task UpdateTodoAsync_Should_Return_False_When_Request_Fails()
        {
            // Arrange
            var todoDto = new TodoDto { Id = "123", Title = "Updated Todo" };
            var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            SetupHttpMessageHandler("todos/123", HttpMethod.Put, httpResponse);

            // Act
            var result = await _service.UpdateTodoAsync(todoDto, _cancellationToken);

            // Assert
            Assert.False(result);

            VerifyLoggerCalled(LogLevel.Information, "Updating todo 123 in JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Error, "Failed to update todo 123 in JSONPlaceholder. Status code: NotFound");
        }

        [Fact]
        public async Task UpdateTodoAsync_Should_Throw_InvalidOperationException_On_HttpRequestException()
        {
            // Arrange
            var todoDto = new TodoDto { Id = "123", Title = "Updated Todo" };
            SetupHttpMessageHandlerToThrow("todos/123", HttpMethod.Put, new HttpRequestException("Network error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateTodoAsync(todoDto, _cancellationToken));

            Assert.Equal("Failed to update todo 123 in external service", exception.Message);
            Assert.IsType<HttpRequestException>(exception.InnerException);

            VerifyLoggerCalled(LogLevel.Information, "Updating todo 123 in JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Error, "HTTP error occurred while updating todo 123 in JSONPlaceholder");
        }

        #endregion

        #region DeleteTodoAsync Tests

        [Fact]
        public async Task DeleteTodoAsync_Should_Return_True_When_Successful()
        {
            // Arrange
            var todoId = 123;
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

            SetupHttpMessageHandler($"todos/{todoId}", HttpMethod.Delete, httpResponse);

            // Act
            var result = await _service.DeleteTodoAsync(todoId, _cancellationToken);

            // Assert
            Assert.True(result);

            VerifyLoggerCalled(LogLevel.Information, $"Deleting todo {todoId} from JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Information, $"Successfully deleted todo {todoId} from JSONPlaceholder");
        }

        [Fact]
        public async Task DeleteTodoAsync_Should_Return_False_When_Request_Fails()
        {
            // Arrange
            var todoId = 123;
            var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

            SetupHttpMessageHandler($"todos/{todoId}", HttpMethod.Delete, httpResponse);

            // Act
            var result = await _service.DeleteTodoAsync(todoId, _cancellationToken);

            // Assert
            Assert.False(result);

            VerifyLoggerCalled(LogLevel.Information, $"Deleting todo {todoId} from JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Error, $"Failed to delete todo {todoId} from JSONPlaceholder. Status code: NotFound");
        }

        [Fact]
        public async Task DeleteTodoAsync_Should_Throw_InvalidOperationException_On_HttpRequestException()
        {
            // Arrange
            var todoId = 123;
            SetupHttpMessageHandlerToThrow($"todos/{todoId}", HttpMethod.Delete, new HttpRequestException("Network error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.DeleteTodoAsync(todoId, _cancellationToken));

            Assert.Equal($"Failed to delete todo {todoId} from external service", exception.Message);
            Assert.IsType<HttpRequestException>(exception.InnerException);

            VerifyLoggerCalled(LogLevel.Information, $"Deleting todo {todoId} from JSONPlaceholder");
            VerifyLoggerCalled(LogLevel.Error, $"HTTP error occurred while deleting todo {todoId} from JSONPlaceholder");
        }

        #endregion

        #region MapToTodoDto Tests

        [Fact]
        public async Task MapToTodoDto_Should_Generate_New_Guid_For_Each_Call()
        {
            // Arrange
            var externalTodos = new[]
            {
                new JsonPlaceholderTodo { Id = 1, Title = "Todo 1", Completed = false },
                new JsonPlaceholderTodo { Id = 2, Title = "Todo 2", Completed = true }
            };

            var jsonResponse = JsonSerializer.Serialize(externalTodos);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            SetupHttpMessageHandler("todos", HttpMethod.Get, httpResponse);

            // Act
            var result = await _service.GetTodosAsync(_cancellationToken);

            // Assert
            var todoList = result.ToList();
            Assert.NotEqual(todoList[0].Id, todoList[1].Id);
            Assert.True(Guid.TryParse(todoList[0].Id, out _));
            Assert.True(Guid.TryParse(todoList[1].Id, out _));
        }

        #endregion

        #region Helper Methods

        private void SetupHttpMessageHandler(string requestUri, HttpMethod method, HttpResponseMessage response)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == method &&
                        req.RequestUri.ToString().EndsWith(requestUri)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }

        private void SetupHttpMessageHandlerToThrow(string requestUri, HttpMethod method, Exception exception)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == method &&
                        req.RequestUri.ToString().EndsWith(requestUri)),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(exception);
        }

        private void VerifyLoggerCalled(LogLevel logLevel, string message)
        {
            _mockLogger.Verify(
                x => x.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        #endregion

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}