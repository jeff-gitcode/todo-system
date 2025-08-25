namespace TodoSystem.Infrastructure.Configuration
{
    public class KafkaConfig
    {
        public const string SectionName = "Kafka";

        public string BootstrapServers { get; set; } = "localhost:9092";
        public string ExternalTodoTopic { get; set; } = "external-todos-created";
        public string ConsumerGroupId { get; set; } = "todo-system-consumers";
        public bool AutoOffsetReset { get; set; } = true;
        public int MessageTimeoutMs { get; set; } = 5000;
        public bool EnableAutoCommit { get; set; } = true;
        public int AutoCommitIntervalMs { get; set; } = 1000;
    }
}