using MediatR;
using TodoSystem.Application.Services;
using Microsoft.Extensions.Logging;

namespace TodoSystem.Application.Todos.Commands
{
    public class DeleteExternalTodoCommand : IRequest<bool>
    {
        public int ExternalId { get; set; }
    }

    public class DeleteExternalTodoCommandHandler : IRequestHandler<DeleteExternalTodoCommand, bool>
    {
        private readonly IExternalTodoService _externalTodoService;
        private readonly ILogger<DeleteExternalTodoCommandHandler> _logger;

        public DeleteExternalTodoCommandHandler(
            IExternalTodoService externalTodoService,
            ILogger<DeleteExternalTodoCommandHandler> logger)
        {
            _externalTodoService = externalTodoService;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteExternalTodoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting external todo {ExternalId}", request.ExternalId);

            // First, check if the external todo exists
            var existingTodo = await _externalTodoService.GetTodoByIdAsync(request.ExternalId, cancellationToken);
            if (existingTodo == null)
            {
                _logger.LogWarning("External todo {ExternalId} not found for deletion", request.ExternalId);
                return false; // Or throw exception depending on your business logic
            }

            var result = await _externalTodoService.DeleteTodoAsync(request.ExternalId, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Successfully deleted external todo {ExternalId}", request.ExternalId);
            }
            else
            {
                _logger.LogError("Failed to delete external todo {ExternalId}", request.ExternalId);
            }

            return result;
        }
    }
}