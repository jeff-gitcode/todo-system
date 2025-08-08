using System;

namespace TodoSystem.Domain.Entities
{

    public class Todo
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public byte[]? RowVersion { get; set; } // For optimistic concurrency
    }
}
