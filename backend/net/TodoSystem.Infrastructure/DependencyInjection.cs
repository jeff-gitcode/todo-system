using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory; // Add this using statement
using Polly;
using Polly.Extensions.Http;
using TodoSystem.Application.Services;
using TodoSystem.Domain.Repositories;
using TodoSystem.Infrastructure.Data;
using TodoSystem.Infrastructure.ExternalServices;
using TodoSystem.Infrastructure.Repositories;
using TodoSystem.Infrastructure.Services;
using TodoSystem.Infrastructure.Configuration;

namespace TodoSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add memory cache services - ADD THIS LINE
            services.AddMemoryCache();

            // Database context
            services.AddDbContext<TodoDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            services.AddScoped<ITodoRepository, TodoRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Configure JsonPlaceholder options
            services.Configure<JsonPlaceholderOptions>(
                configuration.GetSection(JsonPlaceholderOptions.SectionName));

            // Register HTTP client with your existing Polly policies
            services.AddHttpClient<JsonPlaceholderService>(client =>
            {
                var options = configuration.GetSection(JsonPlaceholderOptions.SectionName).Get<JsonPlaceholderOptions>()
                    ?? new JsonPlaceholderOptions();

                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "TodoSystem/1.0");
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetTimeoutPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Register the base service
            services.AddScoped<JsonPlaceholderService>();

            // Register cache service first
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // Register the decorator manually
            services.AddScoped<IExternalTodoService>(serviceProvider =>
            {
                var baseService = serviceProvider.GetRequiredService<JsonPlaceholderService>();
                var cacheService = serviceProvider.GetRequiredService<ICacheService>();
                var logger = serviceProvider.GetRequiredService<ILogger<ExternalTodoServiceCachingDecorator>>();

                return new ExternalTodoServiceCachingDecorator(baseService, cacheService, logger);
            });

            // Register Kafka configuration
            services.Configure<KafkaConfig>(configuration.GetSection("Kafka"));

            // Register Kafka producer
            services.AddSingleton<IEventPublisher, KafkaProducerService>();

            // Register Kafka consumer as hosted service (if you want it to run in the background)
            // services.AddHostedService<KafkaConsumerService>();

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // Retry 3 times with exponential backoff on transient HTTP errors
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            // Per-try timeout of 10 seconds
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            // Circuit breaker: break for 30 seconds after 3 consecutive failures
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30));
        }
    }
}