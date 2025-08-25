using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Services;
using TodoSystem.Infrastructure.ExternalServices.Models;

namespace TodoSystem.Infrastructure.ExternalServices
{
    public class JsonPlaceholderService : IExternalTodoService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<JsonPlaceholderService> _logger;

        public JsonPlaceholderService(
            HttpClient httpClient,
            ILogger<JsonPlaceholderService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<TodoDto>> GetTodosAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching todos from JSONPlaceholder");

                var response = await _httpClient.GetFromJsonAsync<JsonPlaceholderTodo[]>(
                    "todos", cancellationToken);

                _logger.LogInformation("Received response from JSONPlaceholder");
                if (response == null)
                {
                    _logger.LogWarning("No todos received from JSONPlaceholder");
                    return Enumerable.Empty<TodoDto>();
                }

                var todos = response.Select(MapToTodoDto);
                _logger.LogInformation("Successfully fetched {Count} todos from JSONPlaceholder", response.Length);

                return todos;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while fetching todos from JSONPlaceholder");
                throw new InvalidOperationException("Failed to fetch todos from external service", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching todos from JSONPlaceholder");
                throw;
            }
        }

        public async Task<TodoDto?> GetTodoByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching todo {TodoId} from JSONPlaceholder", id);

                var response = await _httpClient.GetFromJsonAsync<JsonPlaceholderTodo>(
                    $"todos/{id.ToString()}", cancellationToken);

                if (response == null)
                {
                    _logger.LogWarning("Todo {TodoId} not found in JSONPlaceholder", id);
                    return null;
                }

                var todo = MapToTodoDto(response);
                _logger.LogInformation("Successfully fetched todo {TodoId} from JSONPlaceholder", id);

                return todo;
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                _logger.LogWarning("Todo {TodoId} not found in JSONPlaceholder", id);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while fetching todo {TodoId} from JSONPlaceholder", id);
                throw new InvalidOperationException($"Failed to fetch todo {id} from external service", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching todo {TodoId} from JSONPlaceholder", id);
                throw;
            }
        }

        public async Task<bool> CreateTodoAsync(TodoDto todo, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Adding new todo to JSONPlaceholder");

                var externalTodo = new JsonPlaceholderTodo
                {
                    Title = todo.Title,
                    Completed = false // Assuming new todos are not completed
                };

                var response = await _httpClient.PostAsJsonAsync("todos", externalTodo, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully added new todo to JSONPlaceholder");
                    return true;
                }

                _logger.LogError("Failed to add new todo to JSONPlaceholder. Status code: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while adding new todo to JSONPlaceholder");
                throw new InvalidOperationException("Failed to add todo to external service", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while adding new todo to JSONPlaceholder");
                throw;
            }
        }

        public async Task<bool> UpdateTodoAsync(TodoDto todo, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating todo {TodoId} in JSONPlaceholder", todo.Id);

                var externalTodo = new JsonPlaceholderTodo
                {
                    Id = int.Parse(todo.Id), // Use hash code of Guid as int Id
                    Title = todo.Title,
                    Completed = false // Assuming we don't update completed status
                };

                var response = await _httpClient.PutAsJsonAsync($"todos/{externalTodo.Id}", externalTodo, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated todo {TodoId} in JSONPlaceholder", todo.Id);
                    return true;
                }

                _logger.LogError("Failed to update todo {TodoId} in JSONPlaceholder. Status code: {StatusCode}", todo.Id, response.StatusCode);
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while updating todo {TodoId} in JSONPlaceholder", todo.Id);
                throw new InvalidOperationException($"Failed to update todo {todo.Id} in external service", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating todo {TodoId} in JSONPlaceholder", todo.Id);
                throw;
            }
        }

        public async Task<bool> DeleteTodoAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting todo {TodoId} from JSONPlaceholder", id);

                var response = await _httpClient.DeleteAsync($"todos/{id}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deleted todo {TodoId} from JSONPlaceholder", id);
                    return true;
                }

                _logger.LogError("Failed to delete todo {TodoId} from JSONPlaceholder. Status code: {StatusCode}", id, response.StatusCode);
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while deleting todo {TodoId} from JSONPlaceholder", id);
                throw new InvalidOperationException($"Failed to delete todo {id} from external service", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting todo {TodoId} from JSONPlaceholder", id);
                throw;
            }
        }

        private static TodoDto MapToTodoDto(JsonPlaceholderTodo external)
        {
            return new TodoDto
            {
                Id = Guid.NewGuid().ToString(), // Generate new GUID for our system
                Title = external.Title
            };
        }
    }
}