using MediatR;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Services;
using Microsoft.Extensions.Logging;

namespace TodoSystem.Application.Todos.Commands
{
    public class CreateExternalTodoCommand : IRequest<TodoDto>
    {
        public string Title { get; set; } = default!;
    }

    public class CreateExternalTodoCommandHandler : IRequestHandler<CreateExternalTodoCommand, TodoDto>
    {
        private readonly IExternalTodoService _externalTodoService;
        private readonly ILogger<CreateExternalTodoCommandHandler> _logger;

        public CreateExternalTodoCommandHandler(
            IExternalTodoService externalTodoService,
            ILogger<CreateExternalTodoCommandHandler> logger)
        {
            _externalTodoService = externalTodoService;
            _logger = logger;
        }

        public async Task<TodoDto> Handle(CreateExternalTodoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating external todo with title: {Title}", request.Title);

            var todoDto = new TodoDto
            {
                Id = Guid.NewGuid(),
                Title = request.Title
            };

            var result = await _externalTodoService.CreateTodoAsync(todoDto, cancellationToken);
            
            if (!result)
            {
                _logger.LogError("Failed to create external todo with title: {Title}", request.Title);
                throw new InvalidOperationException("Failed to create todo in external service");
            }

            _logger.LogInformation("Successfully created external todo with title: {Title}", request.Title);
            return todoDto;
        }
    }
}