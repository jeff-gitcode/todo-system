using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using TodoSystem.Application.Services;
using TodoSystem.Domain.Repositories;
using TodoSystem.Infrastructure.Data;
using TodoSystem.Infrastructure.ExternalServices;
using TodoSystem.Infrastructure.Repositories;

namespace TodoSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register infrastructure services here, for example:

            services.AddDbContext<TodoDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<ITodoRepository, TodoRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Configure JsonPlaceholder options
            services.Configure<JsonPlaceholderOptions>(
                configuration.GetSection(JsonPlaceholderOptions.SectionName));

            services.AddHttpClient<IExternalTodoService, JsonPlaceholderService>((sp, client) =>
            {
                var options = configuration.GetSection(JsonPlaceholderOptions.SectionName).Get<JsonPlaceholderOptions>()
                    ?? new JsonPlaceholderOptions();

                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "TodoSystem/1.0");
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetTimeoutPolicy());

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // Retry 3 times with exponential backoff on 5xx and 408
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            // Per-try timeout
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
        }
    }
}
