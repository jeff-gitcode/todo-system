using MediatR;
using TodoSystem.Application.Dtos;
using System.Collections.Generic;

namespace TodoSystem.Application.Todos.Queries
{
    public class GetTodosQuery : IRequest<IEnumerable<TodoDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Filter { get; set; }
        public string? Sort { get; set; }
    }
}
