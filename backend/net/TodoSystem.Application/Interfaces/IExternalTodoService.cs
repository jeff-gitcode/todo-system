using TodoSystem.Application.Dtos;

namespace TodoSystem.Application.Services
{
    public interface IExternalTodoService
    {
        Task<IEnumerable<TodoDto>> GetTodosAsync(CancellationToken cancellationToken = default);
        Task<TodoDto?> GetTodoByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> CreateTodoAsync(TodoDto todo, CancellationToken cancellationToken = default);
        Task<bool> UpdateTodoAsync(TodoDto todo, CancellationToken cancellationToken = default);
        Task<bool> DeleteTodoAsync(int id, CancellationToken cancellationToken = default);
    }
}