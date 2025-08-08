using Xunit;
using Moq;
using AutoMapper;
using System;
using System.Threading;
using System.Threading.Tasks;
using TodoSystem.Application.Todos.Queries;
using TodoSystem.Application.Dtos;
using TodoSystem.Domain.Entities;
using TodoSystem.Domain.Repositories;

namespace TodoSystem.Application.Tests.Todos.Queries
{
    public class GetTodoByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Return_TodoDto_When_Todo_Exists()
        {
            // Arrange
            var mockRepo = new Mock<ITodoRepository>();
            var mockMapper = new Mock<IMapper>();
            var handler = new GetTodoByIdQueryHandler(mockRepo.Object, mockMapper.Object);

            var todoId = Guid.NewGuid();
            var todo = new Todo { Id = todoId, Title = "Test Todo" };
            var todoDto = new TodoDto { Id = todoId, Title = "Test Todo" };

            mockRepo.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync(todo);
            mockMapper.Setup(m => m.Map<TodoDto>(todo)).Returns(todoDto);

            var query = new GetTodoByIdQuery(todoId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(todoId, result.Id);
            Assert.Equal(todo.Title, result.Title);
            mockRepo.Verify(r => r.GetByIdAsync(todoId), Times.Once);
            mockMapper.Verify(m => m.Map<TodoDto>(todo), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Todo_Does_Not_Exist()
        {
            // Arrange
            var mockRepo = new Mock<ITodoRepository>();
            var mockMapper = new Mock<IMapper>();
            var handler = new GetTodoByIdQueryHandler(mockRepo.Object, mockMapper.Object);

            var todoId = Guid.NewGuid();
            mockRepo.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync((Todo?)null);

            var query = new GetTodoByIdQuery(todoId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
            mockRepo.Verify(r => r.GetByIdAsync(todoId), Times.Once);
            mockMapper.Verify(m => m.Map<TodoDto>(It.IsAny<Todo>()), Times.Never);
        }
    }
}