using Xunit;
using Moq;
using AutoMapper;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoSystem.Application.Todos.Queries;
using TodoSystem.Application.Dtos;
using TodoSystem.Domain.Entities;
using TodoSystem.Domain.Repositories;
using System.Linq;

namespace TodoSystem.Application.Tests.Todos.Queries
{
    public class GetTodosQueryHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Return_Mapped_TodoDtos()
        {
            // Arrange
            var mockRepo = new Mock<ITodoRepository>();
            var mockMapper = new Mock<IMapper>();
            var handler = new GetTodosQueryHandler(mockRepo.Object, mockMapper.Object);

            var todos = new List<Todo>
            {
                new Todo { Id = System.Guid.NewGuid(), Title = "Todo 1" },
                new Todo { Id = System.Guid.NewGuid(), Title = "Todo 2" }
            };
            var todoDtos = todos.Select(t => new TodoDto { Id = t.Id, Title = t.Title }).ToList();

            var query = new GetTodosQuery { Page = 1, PageSize = 10, Filter = null, Sort = null };

            mockRepo.Setup(r => r.GetPagedAsync(1, 10, null, null)).ReturnsAsync(todos);
            mockMapper.Setup(m => m.Map<IEnumerable<TodoDto>>(todos)).Returns(todoDtos);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal(todoDtos[0].Title, result.First().Title);
            mockRepo.Verify(r => r.GetPagedAsync(1, 10, null, null), Times.Once);
            mockMapper.Verify(m => m.Map<IEnumerable<TodoDto>>(todos), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_When_No_Todos()
        {
            // Arrange
            var mockRepo = new Mock<ITodoRepository>();
            var mockMapper = new Mock<IMapper>();
            var handler = new GetTodosQueryHandler(mockRepo.Object, mockMapper.Object);

            var todos = new List<Todo>();
            var todoDtos = new List<TodoDto>();

            var query = new GetTodosQuery { Page = 2, PageSize = 5, Filter = "none", Sort = "title" };

            mockRepo.Setup(r => r.GetPagedAsync(2, 5, "none", "title")).ReturnsAsync(todos);
            mockMapper.Setup(m => m.Map<IEnumerable<TodoDto>>(todos)).Returns(todoDtos);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            mockRepo.Verify(r => r.GetPagedAsync(2, 5, "none", "title"), Times.Once);
            mockMapper.Verify(m => m.Map<IEnumerable<TodoDto>>(todos), Times.Once);
        }
    }
}