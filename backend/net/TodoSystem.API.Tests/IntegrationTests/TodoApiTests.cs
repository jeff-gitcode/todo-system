using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TodoSystem.API;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Todos.Commands;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Security.Claims;
using TodoSystem.Application.Todos.Queries;
using System.Net.Http;

namespace TodoSystem.API.IntegrationTests;

public class TodoApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TodoApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real services with mocks
                var mediatorDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IMediator));

                if (mediatorDescriptor != null)
                {
                    services.Remove(mediatorDescriptor);
                }

                // Add mocked MediatR
                var mockMediator = new Mock<IMediator>();
                mockMediator.Setup(m => m.Send(It.IsAny<CreateTodoCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new TodoDto { Id = Guid.NewGuid().ToString(), Title = "Test" });

                services.AddSingleton(mockMediator.Object);

                // Replace JWT authentication with test authentication
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // Override authorization to allow all requests
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes("Test")
                        .Build();
                });
            });
        });
    }

    [Fact]
    public async Task Post_Todo_Returns_Created()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        var command = new CreateTodoCommand { Title = "Test" };

        // Add JWT token to bypass authorization
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Test", "test-token");

        // Add antiforgery token
        await AddAntiforgeryTokenAsync(client);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/todos", command);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Get_Todos_Returns_Ok()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var todoList = new[] { new TodoDto { Id = Guid.NewGuid().ToString(), Title = "Test Todo" } };

        // Setup the factory with specific mediator behavior for this test
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var mediatorDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (mediatorDescriptor != null)
                {
                    services.Remove(mediatorDescriptor);
                }

                mockMediator.Setup(m => m.Send(It.IsAny<GetTodosQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(todoList);

                services.AddSingleton(mockMediator.Object);
            });
        });

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost") // Ensure HTTPS for antiforgery
        });

        // Add JWT token to bypass authorization
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Test", "test-token");

        // Act
        var response = await client.GetAsync("/api/v1/todos");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todos = await response.Content.ReadFromJsonAsync<TodoDto[]>();
        Assert.NotNull(todos);
        Assert.Single(todos);
        Assert.Equal("Test Todo", todos[0].Title);
    }

    [Fact]
    public async Task Get_Todo_By_Id_Returns_Ok_When_Found()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var mockMediator = new Mock<IMediator>();
        var todo = new TodoDto { Id = todoId.ToString(), Title = "Test Todo" };

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var mediatorDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (mediatorDescriptor != null)
                {
                    services.Remove(mediatorDescriptor);
                }

                mockMediator.Setup(m => m.Send(It.Is<GetTodoByIdQuery>(q => q.Id == todoId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(todo);

                services.AddSingleton(mockMediator.Object);
            });
        });

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost") // Ensure HTTPS for antiforgery
        });

        // Add JWT token to bypass authorization
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Test", "test-token");

        // Act
        var response = await client.GetAsync($"/api/v1/todos/{todoId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returnedTodo = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(returnedTodo);
        Assert.Equal(todoId.ToString(), returnedTodo.Id);
        Assert.Equal("Test Todo", returnedTodo.Title);
    }

    [Fact]
    public async Task Get_Todo_By_Id_Returns_NotFound_When_Not_Exists()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var mockMediator = new Mock<IMediator>();

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var mediatorDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (mediatorDescriptor != null)
                {
                    services.Remove(mediatorDescriptor);
                }

                mockMediator.Setup(m => m.Send(It.Is<GetTodoByIdQuery>(q => q.Id == todoId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TodoDto)null);

                services.AddSingleton(mockMediator.Object);
            });
        });

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost") // Ensure HTTPS for antiforgery
        });

        // Add JWT token to bypass authorization
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Test", "test-token");

        // Act
        var response = await client.GetAsync($"/api/v1/todos/{todoId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_Todo_Returns_Ok()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var mockMediator = new Mock<IMediator>();
        var updatedTodo = new TodoDto { Id = todoId.ToString(), Title = "Updated Todo" };

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var mediatorDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (mediatorDescriptor != null)
                {
                    services.Remove(mediatorDescriptor);
                }

                mockMediator.Setup(m => m.Send(It.IsAny<UpdateTodoCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(updatedTodo);

                services.AddSingleton(mockMediator.Object);
            });
        });

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost") // Ensure HTTPS for antiforgery
        });
        var command = new UpdateTodoCommand { Id = todoId, Title = "Updated Todo" };

        // Add JWT token to bypass authorization
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Test", "test-token");

        // Add antiforgery token
        await AddAntiforgeryTokenAsync(client);

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/todos/{todoId}", command);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returnedTodo = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(returnedTodo);
        Assert.Equal(todoId.ToString(), returnedTodo.Id);
        Assert.Equal("Updated Todo", returnedTodo.Title);
    }

    [Fact]
    public async Task Delete_Todo_Returns_NoContent()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var mockMediator = new Mock<IMediator>();

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var mediatorDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (mediatorDescriptor != null)
                {
                    services.Remove(mediatorDescriptor);
                }

                mockMediator.Setup(m => m.Send(It.Is<DeleteTodoCommand>(c => c.Id == todoId), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(mockMediator.Object);
            });
        });

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost") // Ensure HTTPS for antiforgery
        });

        // Add JWT token to bypass authorization
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Test", "test-token");

        // Add antiforgery token
        await AddAntiforgeryTokenAsync(client);

        // Act
        var response = await client.DeleteAsync($"/api/v1/todos/{todoId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // Add this helper method to your test class
    private async Task AddAntiforgeryTokenAsync(HttpClient client)
    {
        // 1. Get the antiforgery token cookie by making a GET request
        var getResponse = await client.GetAsync("/api/v1/todos");
        getResponse.EnsureSuccessStatusCode();

        // 2. Extract the XSRF-TOKEN cookie
        var setCookie = getResponse.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(x => x.StartsWith("XSRF-TOKEN"));
        if (setCookie == null)
            throw new InvalidOperationException("No XSRF-TOKEN cookie found.");

        // Manually parse the XSRF-TOKEN value from the Set-Cookie header
        var xsrfToken = setCookie.Split(';')
            .Select(part => part.Trim())
            .FirstOrDefault(part => part.StartsWith("XSRF-TOKEN=", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(xsrfToken))
            throw new InvalidOperationException("No XSRF-TOKEN value found.");

        xsrfToken = xsrfToken.Substring("XSRF-TOKEN=".Length);

        // 3. Add the XSRF-TOKEN cookie and header to the client
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", xsrfToken);

        // Add the cookie to the CookieContainer if using HttpClientHandler, or use a Cookie header
        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add("Cookie", $"XSRF-TOKEN={xsrfToken}");
    }
}

// Test authentication handler that always succeeds
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Create test claims principal
        var claims = new[] { new Claim(ClaimTypes.Name, "Test User") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        // Return successful authentication result
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}