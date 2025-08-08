using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TodoSystem.Infrastructure.Data
{
    public class TodoDbContextFactory : IDesignTimeDbContextFactory<TodoDbContext>
    {
        public TodoDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TodoDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? "Host=localhost;Port=5432;Database=todos;Username=postgres;Password=postgres";

            optionsBuilder.UseNpgsql(connectionString);

            return new TodoDbContext(optionsBuilder.Options);
        }
    }
}