using TodoSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoSystem.Domain.Repositories
{
    public interface ITodoRepository
    {
        Task<Todo?> GetByIdAsync(Guid id);
        Task<IEnumerable<Todo>> GetPagedAsync(int page, int pageSize, string? filter, string? sort);
        Task AddAsync(Todo todo);
        Task UpdateAsync(Todo todo);
        Task DeleteAsync(Guid id);
    }
}
