using System;

namespace TodoSystem.Domain.Events
{
    public class ExternalTodoCreatedEvent
    {
        public string Id { get; set; } = default!;
        public string Title { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string Source { get; set; } = "JSONPlaceholder";
        public string EventType { get; set; } = "ExternalTodoCreated";
        public string CorrelationId { get; set; } = default!;
    }
}