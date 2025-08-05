using TodoSystem.Domain.Entities;
using TodoSystem.Domain.Repositories;
using TodoSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoSystem.Infrastructure.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly TodoDbContext _context;
        public TodoRepository(TodoDbContext context) => _context = context;

        public async Task<Todo?> GetByIdAsync(Guid id) =>
            await _context.Todos.FindAsync(id);

        public async Task<IEnumerable<Todo>> GetPagedAsync(int page, int pageSize, string? filter, string? sort)
        {
            var query = _context.Todos.AsQueryable();
            // Filtering, sorting logic here...
            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task AddAsync(Todo todo)
        {
            await _context.Todos.AddAsync(todo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Todo todo)
        {
            _context.Todos.Update(todo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo != null)
            {
                _context.Todos.Remove(todo);
                await _context.SaveChangesAsync();
            }
        }
    }
}
