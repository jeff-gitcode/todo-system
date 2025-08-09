using MediatR;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Services;

namespace TodoSystem.Application.Todos.Queries
{
    public record GetExternalTodosQuery : IRequest<IEnumerable<TodoDto>>;

    public class GetExternalTodosQueryHandler : IRequestHandler<GetExternalTodosQuery, IEnumerable<TodoDto>>
    {
        private readonly IExternalTodoService _externalTodoService;

        public GetExternalTodosQueryHandler(IExternalTodoService externalTodoService)
        {
            _externalTodoService = externalTodoService;
        }

        public async Task<IEnumerable<TodoDto>> Handle(GetExternalTodosQuery request, CancellationToken cancellationToken)
        {
            return await _externalTodoService.GetTodosAsync(cancellationToken);
        }
    }
}