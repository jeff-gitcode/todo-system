using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            // Configure JsonPlaceholder options - Fixed the binding
            // services.Configure<JsonPlaceholderOptions>(
            //     configuration.GetSection(JsonPlaceholderOptions.SectionName));

            // Register HttpClient for JsonPlaceholder
            services.AddHttpClient<IExternalTodoService, JsonPlaceholderService>(client =>
            {
                var options = configuration.GetSection(JsonPlaceholderOptions.SectionName).Get<JsonPlaceholderOptions>()
                    ?? new JsonPlaceholderOptions();

                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "TodoSystem/1.0");
            });

            return services;
        }
    }
}
