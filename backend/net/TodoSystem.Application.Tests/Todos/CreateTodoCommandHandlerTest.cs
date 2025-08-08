using Xunit;
using Moq;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using TodoSystem.Application.Todos.Commands;
using TodoSystem.Application.Dtos;
using TodoSystem.Domain.Entities;
using TodoSystem.Domain.Repositories;
using FluentValidation;

namespace TodoSystem.Application.Tests.Todos.Commands
{
    public class CreateTodoCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Create_Todo_And_Return_Dto()
        {
            // Arrange
            var mockRepo = new Mock<ITodoRepository>();
            var mockMapper = new Mock<IMapper>();
            var handler = new CreateTodoCommandHandler(mockRepo.Object, mockMapper.Object);

            var command = new CreateTodoCommand { Title = "Test Todo" };
            var todo = new Todo { Id = System.Guid.NewGuid(), Title = command.Title };
            var todoDto = new TodoDto { Id = todo.Id, Title = todo.Title };

            mockRepo.Setup(r => r.AddAsync(It.IsAny<Todo>())).Returns(Task.CompletedTask);
            mockMapper.Setup(m => m.Map<TodoDto>(It.IsAny<Todo>())).Returns(todoDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.Title, result.Title);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Todo>()), Times.Once);
            mockMapper.Verify(m => m.Map<TodoDto>(It.IsAny<Todo>()), Times.Once);
        }
    }

    public class CreateTodoCommandValidatorTests
    {
        [Fact]
        public void Validator_Should_Fail_When_Title_Is_Empty()
        {
            // Arrange
            var validator = new CreateTodoCommandValidator();
            var command = new CreateTodoCommand { Title = "" };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Title");
        }

        [Fact]
        public void Validator_Should_Fail_When_Title_Too_Long()
        {
            // Arrange
            var validator = new CreateTodoCommandValidator();
            var command = new CreateTodoCommand { Title = new string('a', 201) };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Title");
        }

        [Fact]
        public void Validator_Should_Pass_With_Valid_Title()
        {
            // Arrange
            var validator = new CreateTodoCommandValidator();
            var command = new CreateTodoCommand { Title = "Valid Title" };

            // Act
            var result = validator.Validate(command);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}