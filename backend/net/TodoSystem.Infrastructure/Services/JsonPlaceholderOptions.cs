namespace TodoSystem.Infrastructure.ExternalServices
{
    public class JsonPlaceholderOptions
    {
        public const string SectionName = "JsonPlaceholder";
        
        public string BaseUrl { get; set; } = "https://jsonplaceholder.typicode.com/";
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryCount { get; set; } = 3;
        public bool EnableCaching { get; set; } = true;
        public int CacheDurationMinutes { get; set; } = 15;
    }
}