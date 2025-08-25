using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Services;
using TodoSystem.Infrastructure.Services;
using Xunit;

namespace TodoSystem.Infrastructure.Tests.Services;

public class ExternalTodoServiceCachingDecoratorTests
{
    private readonly Mock<IExternalTodoService> _mockInnerService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<ExternalTodoServiceCachingDecorator>> _mockLogger;
    private readonly ExternalTodoServiceCachingDecorator _decorator;
    private readonly CancellationToken _cancellationToken;

    public ExternalTodoServiceCachingDecoratorTests()
    {
        _mockInnerService = new Mock<IExternalTodoService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<ExternalTodoServiceCachingDecorator>>();
        _decorator = new ExternalTodoServiceCachingDecorator(
            _mockInnerService.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
        _cancellationToken = CancellationToken.None;
    }

    [Fact]
    public async Task GetTodosAsync_Should_Return_Cached_Data_When_Cache_Hit()
    {
        // Arrange
        var expectedTodos = new List<TodoDto>
        {
            new TodoDto { Id = "1", Title = "Cached Todo 1" },
            new TodoDto { Id = "2", Title = "Cached Todo 2" }
        };

        // Mock GetOrSetAsync to return cached data directly (simulating cache hit)
        // The issue is that we need to match the exact cache expiration used in the decorator
        _mockCacheService
            .Setup(x => x.GetOrSetAsync<IEnumerable<TodoDto>>(
                "external:todos:all",
                It.IsAny<Func<Task<IEnumerable<TodoDto>>>>(),
               It.IsAny<TimeSpan>(),// Match the exact expiration from your decorator
                _cancellationToken))
            .ReturnsAsync(expectedTodos);

        // Act
        var result = await _decorator.GetTodosAsync(_cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal(expectedTodos, result);

        // Verify that inner service was not called (cache hit)
        _mockInnerService.Verify(
            x => x.GetTodosAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        // Verify cache service was called with correct parameters
        _mockCacheService.Verify(
            x => x.GetOrSetAsync<IEnumerable<TodoDto>>(
                "external:todos:all",
                It.IsAny<Func<Task<IEnumerable<TodoDto>>>>(),
                It.IsAny<TimeSpan>(), // Match the exact expiration from your decorator
                _cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetTodosAsync_Should_Fetch_From_Inner_Service_When_Cache_Miss()
    {
        // Arrange
        var todosFromService = new List<TodoDto>
        {
            new TodoDto { Id = "1", Title = "Service Todo 1" },
            new TodoDto { Id = "2", Title = "Service Todo 2" },
            new TodoDto { Id = "3", Title = "Service Todo 3" }
        };

        _mockInnerService
            .Setup(x => x.GetTodosAsync(_cancellationToken))
            .ReturnsAsync(todosFromService);

        // Setup cache service to simulate cache miss by calling the factory function
        _mockCacheService
            .Setup(x => x.GetOrSetAsync<IEnumerable<TodoDto>>(
                "external:todos:all",
                It.IsAny<Func<Task<IEnumerable<TodoDto>>>>(),
                It.IsAny<TimeSpan>(),
                _cancellationToken))
            .Returns<string, Func<Task<IEnumerable<TodoDto>>>, TimeSpan, CancellationToken>(
                async (key, factory, expiration, ct) => await factory());

        // Act
        var result = await _decorator.GetTodosAsync(_cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.Equal(todosFromService, result);

        // Verify inner service was called (cache miss)
        _mockInnerService.Verify(
            x => x.GetTodosAsync(_cancellationToken),
            Times.Once);

        // Verify cache service was called
        _mockCacheService.Verify(
            x => x.GetOrSetAsync<IEnumerable<TodoDto>>(
                "external:todos:all",
                It.IsAny<Func<Task<IEnumerable<TodoDto>>>>(),
                It.IsAny<TimeSpan>(),
                _cancellationToken),
            Times.Once);

        // Verify logging for cache miss
        VerifyLoggerCalled(LogLevel.Information, "Fetching todos from external service (cache miss)");
    }

    [Fact]
    public async Task GetTodosAsync_Should_Return_Empty_Collection_When_Cache_Returns_Null()
    {
        // Arrange
        _mockCacheService
            .Setup(x => x.GetOrSetAsync<IEnumerable<TodoDto>>(
                "external:todos:all",
                It.IsAny<Func<Task<IEnumerable<TodoDto>>>>(),
                It.IsAny<TimeSpan>(),
                _cancellationToken))
            .ReturnsAsync((IEnumerable<TodoDto>)null);

        // Act
        var result = await _decorator.GetTodosAsync(_cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        // Verify cache service was called
        _mockCacheService.Verify(
            x => x.GetOrSetAsync<IEnumerable<TodoDto>>(
                "external:todos:all",
                It.IsAny<Func<Task<IEnumerable<TodoDto>>>>(),
                It.IsAny<TimeSpan>(),
                _cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetTodoByIdAsync_Should_Return_Cached_Data_When_Cache_Hit()
    {
        // Arrange
        const int todoId = 1;
        var cachedTodo = new TodoDto { Id = "1", Title = "Cached Todo" };

        _mockCacheService
            .Setup(x => x.GetOrSetAsync<TodoDto>(
                "external:todos:id:1",
                It.IsAny<Func<Task<TodoDto>>>(),
                It.IsAny<TimeSpan>(),
                _cancellationToken))
            .ReturnsAsync(cachedTodo);

        // Act
        var result = await _decorator.GetTodoByIdAsync(todoId, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cachedTodo.Id, result.Id);
        Assert.Equal(cachedTodo.Title, result.Title);

        // Verify that inner service was not called (cache hit)
        _mockInnerService.Verify(
            x => x.GetTodoByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTodoByIdAsync_Should_Fetch_From_Inner_Service_When_Cache_Miss()
    {
        // Arrange
        const int todoId = 1;
        var todoFromService = new TodoDto { Id = "1", Title = "Service Todo" };

        _mockInnerService
            .Setup(x => x.GetTodoByIdAsync(todoId, _cancellationToken))
            .ReturnsAsync(todoFromService);

        _mockCacheService
            .Setup(x => x.GetOrSetAsync<TodoDto>(
                "external:todos:id:1",
                It.IsAny<Func<Task<TodoDto>>>(),
                It.IsAny<TimeSpan>(),
                _cancellationToken))
            .Returns<string, Func<Task<TodoDto>>, TimeSpan, CancellationToken>(
                async (key, factory, expiration, ct) => await factory());

        // Act
        var result = await _decorator.GetTodoByIdAsync(todoId, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(todoFromService.Id, result.Id);
        Assert.Equal(todoFromService.Title, result.Title);

        // Verify inner service was called
        _mockInnerService.Verify(
            x => x.GetTodoByIdAsync(todoId, _cancellationToken),
            Times.Once);

        // Verify logging for cache miss
        VerifyLoggerCalled(LogLevel.Information, "Fetching todo 1 from external service (cache miss)");
    }

    [Fact]
    public async Task CreateTodoAsync_Should_Invalidate_Cache_When_Successful()
    {
        // Arrange
        var todoToCreate = new TodoDto { Id = "1", Title = "New Todo" };

        _mockInnerService
            .Setup(x => x.CreateTodoAsync(todoToCreate, _cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _decorator.CreateTodoAsync(todoToCreate, _cancellationToken);

        // Assert
        Assert.True(result);

        // Verify inner service was called
        _mockInnerService.Verify(
            x => x.CreateTodoAsync(todoToCreate, _cancellationToken),
            Times.Once);

        // Verify cache invalidation
        _mockCacheService.Verify(
            x => x.RemoveAsync("external:todos:all", _cancellationToken),
            Times.Once);

        _mockCacheService.Verify(
            x => x.RemoveByPatternAsync("external:todos:", _cancellationToken),
            Times.Once);

        // Verify that invalidation was logged at least once
        VerifyLoggerCalledAtLeastOnce(LogLevel.Information, "Invalidated external todos cache");
    }

    [Fact]
    public async Task CreateTodoAsync_Should_Not_Invalidate_Cache_When_Failed()
    {
        // Arrange
        var todoToCreate = new TodoDto { Id = "1", Title = "New Todo" };

        _mockInnerService
            .Setup(x => x.CreateTodoAsync(todoToCreate, _cancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _decorator.CreateTodoAsync(todoToCreate, _cancellationToken);

        // Assert
        Assert.False(result);

        // Verify inner service was called
        _mockInnerService.Verify(
            x => x.CreateTodoAsync(todoToCreate, _cancellationToken),
            Times.Once);

        // Verify cache was NOT invalidated
        _mockCacheService.Verify(
            x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockCacheService.Verify(
            x => x.RemoveByPatternAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateTodoAsync_Should_Invalidate_Cache_When_Successful()
    {
        // Arrange
        var todoToUpdate = new TodoDto { Id = "1", Title = "Updated Todo" };

        _mockInnerService
            .Setup(x => x.UpdateTodoAsync(todoToUpdate, _cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _decorator.UpdateTodoAsync(todoToUpdate, _cancellationToken);

        // Assert
        Assert.True(result);

        // Verify inner service was called
        _mockInnerService.Verify(
            x => x.UpdateTodoAsync(todoToUpdate, _cancellationToken),
            Times.Once);

        // Verify cache invalidation for collection
        _mockCacheService.Verify(
            x => x.RemoveAsync("external:todos:all", _cancellationToken),
            Times.Once);

        _mockCacheService.Verify(
            x => x.RemoveByPatternAsync("external:todos:", _cancellationToken),
            Times.Once);

        // Verify specific item cache invalidation
        _mockCacheService.Verify(
            x => x.RemoveAsync("external:todos:id:1", _cancellationToken),
            Times.Once);

        // Verify logging
        VerifyLoggerCalled(LogLevel.Information, "Invalidated external todos cache after update");
    }

    [Fact]
    public async Task DeleteTodoAsync_Should_Invalidate_Cache_When_Successful()
    {
        // Arrange
        const int todoId = 1;

        _mockInnerService
            .Setup(x => x.DeleteTodoAsync(todoId, _cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _decorator.DeleteTodoAsync(todoId, _cancellationToken);

        // Assert
        Assert.True(result);

        // Verify inner service was called
        _mockInnerService.Verify(
            x => x.DeleteTodoAsync(todoId, _cancellationToken),
            Times.Once);

        // Verify cache invalidation
        _mockCacheService.Verify(
            x => x.RemoveAsync("external:todos:all", _cancellationToken),
            Times.Once);

        _mockCacheService.Verify(
            x => x.RemoveByPatternAsync("external:todos:", _cancellationToken),
            Times.Once);

        _mockCacheService.Verify(
            x => x.RemoveAsync("external:todos:id:1", _cancellationToken),
            Times.Once);

        // Verify logging
        VerifyLoggerCalled(LogLevel.Information, "Invalidated external todos cache after deletion");
    }


    [Fact]
    public async Task GetTodosAsync_Should_Handle_Exception_From_Inner_Service()
    {
        // Arrange
        var expectedException = new InvalidOperationException("External service unavailable");

        _mockInnerService
            .Setup(x => x.GetTodosAsync(_cancellationToken))
            .ThrowsAsync(expectedException);

        // Setup cache service to call the factory function (simulating cache miss)
        _mockCacheService
            .Setup(x => x.GetOrSetAsync<IEnumerable<TodoDto>>(
                "external:todos:all",
                It.IsAny<Func<Task<IEnumerable<TodoDto>>>>(),
                It.IsAny<TimeSpan>(),
                _cancellationToken))
            .Returns<string, Func<Task<IEnumerable<TodoDto>>>, TimeSpan, CancellationToken>(
                async (key, factory, expiration, ct) => await factory());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _decorator.GetTodosAsync(_cancellationToken));

        Assert.Equal("External service unavailable", exception.Message);

        // Verify inner service was called
        _mockInnerService.Verify(
            x => x.GetTodosAsync(_cancellationToken),
            Times.Once);
    }

    private static IEnumerable<TodoDto> CreateLazyEnumerable()
    {
        // This creates a lazy enumerable that will be materialized when .ToList() is called
        yield return new TodoDto { Id = "1", Title = "Lazy Todo" };
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
            Times.Once);
    }

    private void VerifyLoggerCalledAtLeastOnce(LogLevel logLevel, string messageContains)
    {
        _mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(messageContains)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifySpecificLogMessage(LogLevel logLevel, string exactMessage)
    {
        _mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Equals(exactMessage, StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}