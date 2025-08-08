using Xunit;
using Moq;
using AutoMapper;
using System;
using System.Threading;
using System.Threading.Tasks;
using TodoSystem.Application.Todos.Commands;
using TodoSystem.Application.Dtos;
using TodoSystem.Domain.Entities;
using TodoSystem.Domain.Repositories;

namespace TodoSystem.Application.Tests.Todos.Commands
{
    public class UpdateTodoCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Update_Todo_And_Return_Dto()
        {
            // Arrange
            var mockRepo = new Mock<ITodoRepository>();
            var mockMapper = new Mock<IMapper>();
            var handler = new UpdateTodoCommandHandler(mockRepo.Object, mockMapper.Object);

            var todoId = Guid.NewGuid();
            var existingTodo = new Todo { Id = todoId, Title = "Old Title" };
            var command = new UpdateTodoCommand { Id = todoId, Title = "New Title" };
            var updatedTodo = new Todo { Id = todoId, Title = command.Title };
            var todoDto = new TodoDto { Id = todoId, Title = command.Title };

            mockRepo.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync(existingTodo);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Todo>())).Returns(Task.CompletedTask);
            mockMapper.Setup(m => m.Map<TodoDto>(It.IsAny<Todo>())).Returns(todoDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.Title, result.Title);
            mockRepo.Verify(r => r.GetByIdAsync(todoId), Times.Once);
            mockRepo.Verify(r => r.UpdateAsync(It.Is<Todo>(t => t.Title == command.Title)), Times.Once);
            mockMapper.Verify(m => m.Map<TodoDto>(It.IsAny<Todo>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Throw_When_Todo_Not_Found()
        {
            // Arrange
            var mockRepo = new Mock<ITodoRepository>();
            var mockMapper = new Mock<IMapper>();
            var handler = new UpdateTodoCommandHandler(mockRepo.Object, mockMapper.Object);

            var todoId = Guid.NewGuid();
            var command = new UpdateTodoCommand { Id = todoId, Title = "New Title" };

            mockRepo.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync((Todo?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        }
    }

    public class UpdateTodoCommandValidatorTests
    {
        [Fact]
        public void Validator_Should_Fail_When_Title_Is_Empty()
        {
            var validator = new UpdateTodoCommandValidator();
            var command = new UpdateTodoCommand { Id = Guid.NewGuid(), Title = "" };

            var result = validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Title");
        }

        [Fact]
        public void Validator_Should_Fail_When_Title_Too_Long()
        {
            var validator = new UpdateTodoCommandValidator();
            var command = new UpdateTodoCommand { Id = Guid.NewGuid(), Title = new string('a', 201) };

            var result = validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Title");
        }

        [Fact]
        public void Validator_Should_Pass_With_Valid_Title()
        {
            var validator = new UpdateTodoCommandValidator();
            var command = new UpdateTodoCommand { Id = Guid.NewGuid(), Title = "Valid Title" };

            var result = validator.Validate(command);

            Assert.True(result.IsValid);
        }
    }
}