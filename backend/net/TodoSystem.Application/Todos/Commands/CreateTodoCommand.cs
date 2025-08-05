using MediatR;
using TodoSystem.Application.Dtos;
using TodoSystem.Domain.Entities;
using System;

namespace TodoSystem.Application.Todos.Commands
{
    public class CreateTodoCommand : IRequest<TodoDto>
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public TodoStatus Status { get; set; }
        public TodoPriority Priority { get; set; }
    }
}
