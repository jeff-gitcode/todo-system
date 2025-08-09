using MediatR;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Services;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace TodoSystem.Application.Todos.Commands
{
    public class UpdateExternalTodoCommand : IRequest<TodoDto>
    {
        public int ExternalId { get; set; }
        public string Title { get; set; } = default!;
    }

    public class UpdateExternalTodoCommandHandler : IRequestHandler<UpdateExternalTodoCommand, TodoDto>
    {
        private readonly IExternalTodoService _externalTodoService;
        private readonly ILogger<UpdateExternalTodoCommandHandler> _logger;

        public UpdateExternalTodoCommandHandler(
            IExternalTodoService externalTodoService,
            ILogger<UpdateExternalTodoCommandHandler> logger)
        {
            _externalTodoService = externalTodoService;
            _logger = logger;
        }

        public async Task<TodoDto> Handle(UpdateExternalTodoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating external todo {ExternalId} with title: {Title}", request.ExternalId, request.Title);

            // First, check if the external todo exists
            var existingTodo = await _externalTodoService.GetTodoByIdAsync(request.ExternalId, cancellationToken);
            if (existingTodo == null)
            {
                _logger.LogWarning("External todo {ExternalId} not found", request.ExternalId);
                throw new ArgumentException($"Todo with external ID {request.ExternalId} not found");
            }

            var todoDto = new TodoDto
            {
                Id = existingTodo.Id,
                Title = request.Title
            };

            var result = await _externalTodoService.UpdateTodoAsync(todoDto, cancellationToken);

            if (!result)
            {
                _logger.LogError("Failed to update external todo {ExternalId}", request.ExternalId);
                throw new InvalidOperationException($"Failed to update todo {request.ExternalId} in external service");
            }

            _logger.LogInformation("Successfully updated external todo {ExternalId}", request.ExternalId);
            return todoDto;
        }
    }

    public class UpdateExternalTodoCommandValidator : AbstractValidator<UpdateExternalTodoCommand>
    {
        public UpdateExternalTodoCommandValidator()
        {
            RuleFor(x => x.ExternalId)
                .NotEmpty().WithMessage("External ID must not be empty");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
        }
    }
}