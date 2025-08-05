using Microsoft.EntityFrameworkCore;
using TodoSystem.Domain.Entities;

namespace TodoSystem.Infrastructure.Data
{
    public class TodoDbContext : DbContext
    {
        public DbSet<Todo> Todos { get; set; }

        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Todo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.RowVersion).IsRowVersion();
                // ...other configurations...
            });
        }
    }
}
