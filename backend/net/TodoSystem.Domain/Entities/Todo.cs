using System;

namespace TodoSystem.Domain.Entities
{
    public enum TodoStatus { Pending, InProgress, Completed }
    public enum TodoPriority { Low, Medium, High }

    public class Todo
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public TodoStatus Status { get; set; }
        public TodoPriority Priority { get; set; }
        public byte[] RowVersion { get; set; } // For optimistic concurrency
    }
}
