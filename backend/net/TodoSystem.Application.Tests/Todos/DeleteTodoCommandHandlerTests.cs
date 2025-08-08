using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using TodoSystem.Application.Todos.Commands;
using TodoSystem.Domain.Repositories;

namespace TodoSystem.Application.Tests.Todos.Commands
{
    public class DeleteTodoCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Delete_Todo()
        {
            // Arrange
            var mockRepo = new Mock<ITodoRepository>();
            var handler = new DeleteTodoCommandHandler(mockRepo.Object);

            var todoId = Guid.NewGuid();
            var command = new DeleteTodoCommand { Id = todoId };

            mockRepo.Setup(r => r.DeleteAsync(todoId)).Returns(Task.CompletedTask);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.DeleteAsync(todoId), Times.Once);
        }
    }
}
