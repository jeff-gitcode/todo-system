using MediatR;
using TodoSystem.Application.Dtos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using TodoSystem.Domain.Repositories;

namespace TodoSystem.Application.Todos.Queries
{
    public class GetTodosQuery : IRequest<IEnumerable<TodoDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Filter { get; set; }
        public string? Sort { get; set; }
    }

    public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, IEnumerable<TodoDto>>
    {
        private readonly ITodoRepository _todoRepository;
        private readonly IMapper _mapper;

        public GetTodosQueryHandler(ITodoRepository todoRepository, IMapper mapper)
        {
            _todoRepository = todoRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TodoDto>> Handle(GetTodosQuery request, CancellationToken cancellationToken)
        {
            var todos = await _todoRepository.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.Filter,
                request.Sort
            );

            return _mapper.Map<IEnumerable<TodoDto>>(todos);
        }
    }
}
