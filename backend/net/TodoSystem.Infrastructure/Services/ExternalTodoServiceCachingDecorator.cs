using Microsoft.Extensions.Logging;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Services;

namespace TodoSystem.Infrastructure.Services;

public class ExternalTodoServiceCachingDecorator : IExternalTodoService
{
    private readonly IExternalTodoService _innerService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ExternalTodoServiceCachingDecorator> _logger;

    private const string CACHE_KEY_ALL_TODOS = "external:todos:all";
    private const string CACHE_KEY_TODO_BY_ID = "external:todos:id:{0}";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public ExternalTodoServiceCachingDecorator(
        IExternalTodoService innerService,
        ICacheService cacheService,
        ILogger<ExternalTodoServiceCachingDecorator> logger)
    {
        _innerService = innerService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<TodoDto>> GetTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _cacheService.GetOrSetAsync(
            CACHE_KEY_ALL_TODOS,
            async () =>
            {
                _logger.LogInformation("Fetching todos from external service (cache miss)");
                var todos = await _innerService.GetTodosAsync(cancellationToken);
                return todos; // Materialize the enumerable
            },
            CacheExpiration,
            cancellationToken) ?? Enumerable.Empty<TodoDto>();
    }

    public async Task<TodoDto?> GetTodoByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = string.Format(CACHE_KEY_TODO_BY_ID, id);

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Fetching todo {TodoId} from external service (cache miss)", id);
                return await _innerService.GetTodoByIdAsync(id, cancellationToken);
            },
            CacheExpiration,
            cancellationToken);
    }

    public async Task<bool> CreateTodoAsync(TodoDto todo, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.CreateTodoAsync(todo, cancellationToken);

        if (result)
        {
            // Invalidate cache when data changes
            await InvalidateCacheAsync();
            _logger.LogInformation("Invalidated external todos cache after creation");
        }

        return result;
    }

    public async Task<bool> UpdateTodoAsync(TodoDto todo, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.UpdateTodoAsync(todo, cancellationToken);

        if (result)
        {
            // Invalidate specific item and collection cache
            await InvalidateCacheAsync();

            // Use the correct property name - likely 'Id' instead of 'ExternalId'
            if (int.TryParse(todo.Id, out var todoId))
            {
                var specificKey = string.Format(CACHE_KEY_TODO_BY_ID, todoId);
                await _cacheService.RemoveAsync(specificKey, cancellationToken);
            }

            _logger.LogInformation("Invalidated external todos cache after update");
        }

        return result;
    }

    public async Task<bool> DeleteTodoAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.DeleteTodoAsync(id, cancellationToken);

        if (result)
        {
            // Invalidate specific item and collection cache
            await InvalidateCacheAsync();
            var specificKey = string.Format(CACHE_KEY_TODO_BY_ID, id);
            await _cacheService.RemoveAsync(specificKey, cancellationToken);
            _logger.LogInformation("Invalidated external todos cache after deletion");
        }

        return result;
    }

    private async Task InvalidateCacheAsync()
    {
        await _cacheService.RemoveAsync(CACHE_KEY_ALL_TODOS);
        await _cacheService.RemoveByPatternAsync("external:todos:");
        _logger.LogInformation("Invalidated external todos cache");
    }
}