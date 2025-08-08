using MediatR;
using TodoSystem.Application.Dtos;
using TodoSystem.Domain.Entities;
using TodoSystem.Domain.Repositories;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;

namespace TodoSystem.Application.Todos.Commands
{
    public class CreateTodoCommand : IRequest<TodoDto>
    {
        public string Title { get; set; } = default!;
    }

    public class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, TodoDto>
    {
        private readonly ITodoRepository _todoRepository;
        private readonly IMapper _mapper;

        public CreateTodoCommandHandler(ITodoRepository todoRepository, IMapper mapper)
        {
            _todoRepository = todoRepository;
            _mapper = mapper;
        }

        public async Task<TodoDto> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
        {
            var todo = new Todo
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
            };

            await _todoRepository.AddAsync(todo);

            return _mapper.Map<TodoDto>(todo);
        }
    }
}
