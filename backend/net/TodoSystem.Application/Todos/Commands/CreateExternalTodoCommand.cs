using MediatR;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Services;
using Microsoft.Extensions.Logging;
using TodoSystem.Domain.Events;

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
        private readonly IEventPublisher _eventPublisher;

        public CreateExternalTodoCommandHandler(
            IExternalTodoService externalTodoService,
            ILogger<CreateExternalTodoCommandHandler> logger,
            IEventPublisher eventPublisher
            )
        {
            _externalTodoService = externalTodoService;
            _logger = logger;
            _eventPublisher = eventPublisher;
        }

        public async Task<TodoDto> Handle(CreateExternalTodoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating external todo with title: {Title}", request.Title);

            var todoDto = new TodoDto
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Title
            };

            var result = await _externalTodoService.CreateTodoAsync(todoDto, cancellationToken);

            if (!result)
            {
                _logger.LogError("Failed to create external todo with title: {Title}", request.Title);
                throw new InvalidOperationException("Failed to create todo in external service");
            }

            // Publish event to Kafka after successful creation
            var eventData = new ExternalTodoCreatedEvent
            {
                Id = todoDto.Id,
                Title = todoDto.Title,
                CorrelationId = Guid.NewGuid().ToString(), // Example correlation ID
                                                          // Add other properties as needed
                CreatedAt = DateTime.UtcNow
            };
            await _eventPublisher.PublishExternalTodoCreatedAsync(eventData, cancellationToken);

            _logger.LogInformation("Successfully created external todo with title: {Title}", request.Title);
            return todoDto;
        }
    }
}