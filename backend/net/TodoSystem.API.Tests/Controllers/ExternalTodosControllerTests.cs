
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoSystem.API.Controllers;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Todos.Commands;
using TodoSystem.Application.Todos.Queries;
using Xunit;

namespace TodoSystem.API.Controllers.Tests
{
    public class ExternalTodosControllerTests
    {
        private static ExternalTodosController CreateController(Mock<IMediator> mockMediator)
        {
            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<ExternalTodosController>>();
            return new ExternalTodosController(mockMediator.Object, mockLogger.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithTodos()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var todos = new List<TodoDto>
                        {
                                new TodoDto { Id = System.Guid.NewGuid().ToString(), Title = "T1" },
                                new TodoDto { Id = System.Guid.NewGuid().ToString(), Title = "T2" }
                        };
            mockMediator.Setup(m => m.Send(It.IsAny<GetExternalTodosQuery>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(todos);
            var controller = CreateController(mockMediator);

            // Act
            var action = await controller.GetAll(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<TodoDto>>(ok.Value);
            Assert.Equal(2, value.Count());
            mockMediator.Verify(m => m.Send(It.IsAny<GetExternalTodosQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var dto = new TodoDto { Id = System.Guid.NewGuid().ToString(), Title = "Item" };
            mockMediator.Setup(m => m.Send(It.Is<GetExternalTodoByIdQuery>(q => q.Id == 5), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(dto);
            var controller = CreateController(mockMediator);

            // Act
            var action = await controller.GetById(5, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var value = Assert.IsType<TodoDto>(ok.Value);
            Assert.Equal(dto.Title, value.Title);
            mockMediator.Verify(m => m.Send(It.Is<GetExternalTodoByIdQuery>(q => q.Id == 5), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<GetExternalTodoByIdQuery>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync((TodoDto?)null);
            var controller = CreateController(mockMediator);

            // Act
            var action = await controller.GetById(404, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action.Result);
            mockMediator.Verify(m => m.Send(It.Is<GetExternalTodoByIdQuery>(q => q.Id == 404), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithLocation_AndBody()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var created = new TodoDto { Id = System.Guid.NewGuid().ToString(), Title = "Created" };
            mockMediator.Setup(m => m.Send(It.IsAny<CreateExternalTodoCommand>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(created);
            var controller = CreateController(mockMediator);
            var cmd = new CreateExternalTodoCommand { Title = "Created" };

            // Act
            var action = await controller.Create(cmd, CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(action.Result);
            Assert.Equal("/api/v1/externaltodos", createdResult.Location);
            var value = Assert.IsType<TodoDto>(createdResult.Value);
            Assert.Equal("Created", value.Title);
            mockMediator.Verify(m => m.Send(It.Is<CreateExternalTodoCommand>(c => c.Title == "Created"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var controller = CreateController(mockMediator);
            var cmd = new UpdateExternalTodoCommand { ExternalId = 2, Title = "X" };

            // Act
            var action = await controller.Update(1, cmd, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(action.Result);
            mockMediator.Verify(m => m.Send(It.IsAny<UpdateExternalTodoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenIdsMatch()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var updated = new TodoDto { Id = System.Guid.NewGuid().ToString(), Title = "Updated" };
            mockMediator.Setup(m => m.Send(It.IsAny<UpdateExternalTodoCommand>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(updated);
            var controller = CreateController(mockMediator);
            var cmd = new UpdateExternalTodoCommand { ExternalId = 7, Title = "Updated" };

            // Act
            var action = await controller.Update(7, cmd, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var value = Assert.IsType<TodoDto>(ok.Value);
            Assert.Equal("Updated", value.Title);
            mockMediator.Verify(m => m.Send(It.Is<UpdateExternalTodoCommand>(c => c.ExternalId == 7 && c.Title == "Updated"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleted()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<DeleteExternalTodoCommand>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(true);
            var controller = CreateController(mockMediator);

            // Act
            var action = await controller.Delete(9, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
            mockMediator.Verify(m => m.Send(It.Is<DeleteExternalTodoCommand>(c => c.ExternalId == 9), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenNotDeleted()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<DeleteExternalTodoCommand>(), It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(false);
            var controller = CreateController(mockMediator);

            // Act
            var action = await controller.Delete(10, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
            mockMediator.Verify(m => m.Send(It.Is<DeleteExternalTodoCommand>(c => c.ExternalId == 10), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}