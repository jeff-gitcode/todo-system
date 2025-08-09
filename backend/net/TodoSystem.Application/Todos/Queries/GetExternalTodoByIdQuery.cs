
using FluentValidation;
using MediatR;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Services;

namespace TodoSystem.Application.Todos.Queries
{
    public record GetExternalTodoByIdQuery(int Id) : IRequest<TodoDto?>;

    public class GetExternalTodoByIdQueryHandler : IRequestHandler<GetExternalTodoByIdQuery, TodoDto?>
    {
        private readonly IExternalTodoService _externalTodoService;

        public GetExternalTodoByIdQueryHandler(IExternalTodoService externalTodoService)
        {
            _externalTodoService = externalTodoService;
        }

        public async Task<TodoDto?> Handle(GetExternalTodoByIdQuery request, CancellationToken cancellationToken)
        {
            return await _externalTodoService.GetTodoByIdAsync(request.Id, cancellationToken);
        }
    }

    public class GetExternalTodoByIdQueryValidator : AbstractValidator<GetExternalTodoByIdQuery>
    {
        public GetExternalTodoByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

