using Xunit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TodoSystem.Domain.Entities;
using TodoSystem.Infrastructure.Data;
using TodoSystem.Infrastructure.Repositories;

namespace TodoSystem.Infrastructure.Repositories
{
    public class UserRepositoryTest
    {
        private TodoDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TodoDbContext(options);
        }

        [Fact]
        public async Task AddAsync_Should_Add_User_And_Set_CreatedAt()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var repo = new UserRepository(context);
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", DisplayName = "Test User", PasswordHash = "dummyhash" };

            // Act
            var result = await repo.AddAsync(user);

            // Assert
            Assert.Single(context.Users);
            Assert.Equal("test@example.com", context.Users.First().Email);
            Assert.True(result.CreatedAt <= DateTime.UtcNow && result.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task GetByEmailAsync_Should_Return_User_When_Exists()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var user = new User { Id = Guid.NewGuid(), Email = "findme@example.com", DisplayName = "Find Me", PasswordHash = "dummyhash" };
            context.Users.Add(user);
            context.SaveChanges();
            var repo = new UserRepository(context);

            // Act
            var result = await repo.GetByEmailAsync("findme@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
        }

        [Fact]
        public async Task GetByEmailAsync_Should_Throw_When_Not_Exists()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var repo = new UserRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.GetByEmailAsync("notfound@example.com"));
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_User_And_Set_UpdatedAt()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var user = new User { Id = Guid.NewGuid(), Email = "update@example.com", DisplayName = "Old Name", PasswordHash = "dummyhash" };
            context.Users.Add(user);
            context.SaveChanges();
            var repo = new UserRepository(context);

            // Act
            user.DisplayName = "New Name";
            await repo.UpdateAsync(user);

            // Assert
            var updated = context.Users.Find(user.Id);
            Assert.Equal("New Name", updated!.DisplayName);
            Assert.True(updated.UpdatedAt <= DateTime.UtcNow && updated.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task EmailExistsAsync_Should_Return_True_If_Email_Exists()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var user = new User { Id = Guid.NewGuid(), Email = "exists@example.com", DisplayName = "Exists", PasswordHash = "dummyhash" };
            context.Users.Add(user);
            context.SaveChanges();
            var repo = new UserRepository(context);

            // Act
            var exists = await repo.EmailExistsAsync("exists@example.com");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task EmailExistsAsync_Should_Return_False_If_Email_Does_Not_Exist()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var repo = new UserRepository(context);

            // Act
            var exists = await repo.EmailExistsAsync("nope@example.com");

            // Assert
            Assert.False(exists);
        }
    }
}