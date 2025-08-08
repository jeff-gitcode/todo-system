using Xunit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;
using TodoSystem.Domain.Entities;
using TodoSystem.Infrastructure.Data;
using TodoSystem.Infrastructure.Repositories;

namespace TodoSystem.Infrastructure.Repositories
{
    public class TodoRepositoryTest
    {
        private TodoDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TodoDbContext(options);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Todo()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var repo = new TodoRepository(context);
            var todo = new Todo { Id = Guid.NewGuid(), Title = "Test" };

            // Act
            await repo.AddAsync(todo);

            // Assert
            Assert.Single(context.Todos);
            Assert.Equal("Test", context.Todos.First().Title);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Todo_When_Exists()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var todo = new Todo { Id = Guid.NewGuid(), Title = "Test" };
            context.Todos.Add(todo);
            context.SaveChanges();
            var repo = new TodoRepository(context);

            // Act
            var result = await repo.GetByIdAsync(todo.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(todo.Id, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var repo = new TodoRepository(context);

            // Act
            var result = await repo.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPagedAsync_Should_Return_Paged_Todos()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            for (int i = 1; i <= 15; i++)
                context.Todos.Add(new Todo { Id = Guid.NewGuid(), Title = $"Todo {i}" });
            context.SaveChanges();
            var repo = new TodoRepository(context);

            // Act
            var result = await repo.GetPagedAsync(2, 5, null, null);

            // Assert
            Assert.Equal(5, result.Count());
            Assert.All(result, t => Assert.StartsWith("Todo", t.Title));
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Todo()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var todo = new Todo { Id = Guid.NewGuid(), Title = "Old" };
            context.Todos.Add(todo);
            context.SaveChanges();
            var repo = new TodoRepository(context);
            todo.Title = "Updated";

            // Act
            await repo.UpdateAsync(todo);

            // Assert
            var updated = context.Todos.Find(todo.Id);
            Assert.Equal("Updated", updated!.Title);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Todo_When_Exists()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var todo = new Todo { Id = Guid.NewGuid(), Title = "ToDelete" };
            context.Todos.Add(todo);
            context.SaveChanges();
            var repo = new TodoRepository(context);

            // Act
            await repo.DeleteAsync(todo.Id);

            // Assert
            Assert.Empty(context.Todos);
        }

        [Fact]
        public async Task DeleteAsync_Should_Do_Nothing_When_Not_Exists()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var repo = new TodoRepository(context);

            // Act
            await repo.DeleteAsync(Guid.NewGuid());

            // Assert
            Assert.Empty(context.Todos);
        }
    }
}