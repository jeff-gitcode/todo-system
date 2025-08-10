using System;
using TodoSystem.Domain.Entities;

namespace TodoSystem.Application.Dtos
{
    public class TodoDto
    {
        public String? Id { get; set; }
        public string Title { get; set; } = default!;

    }
}