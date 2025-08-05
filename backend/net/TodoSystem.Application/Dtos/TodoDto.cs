using System;
using TodoSystem.Domain.Entities;

namespace TodoSystem.Application.Dtos
{
    public class TodoDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public TodoStatus Status { get; set; }
        public TodoPriority Priority { get; set; }
    }
}
