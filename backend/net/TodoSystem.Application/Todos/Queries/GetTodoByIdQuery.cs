using MediatR;
using TodoSystem.Application.Dtos;
using TodoSystem.Domain.Repositories;
using AutoMapper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TodoSystem.Application.Todos.Queries
{
    public class GetTodoByIdQuery : IRequest<TodoDto?>
    {
        public Guid Id { get; set; }
        public GetTodoByIdQuery(Guid id) => Id = id;
    }

    public class GetTodoByIdQueryHandler : IRequestHandler<GetTodoByIdQuery, TodoDto?>
    {
        private readonly ITodoRepository _todoRepository;
        private readonly IMapper _mapper;

        public GetTodoByIdQueryHandler(ITodoRepository todoRepository, IMapper mapper)
        {
            _todoRepository = todoRepository;
            _mapper = mapper;
        }

        public async Task<TodoDto?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
        {
            var todo = await _todoRepository.GetByIdAsync(request.Id);
            return todo == null ? null : _mapper.Map<TodoDto>(todo);
        }
    }
}