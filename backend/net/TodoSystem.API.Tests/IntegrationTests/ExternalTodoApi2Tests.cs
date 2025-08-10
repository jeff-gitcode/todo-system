using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using TodoSystem.API;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Todos.Commands;
using TodoSystem.Application.Todos.Queries;
using Xunit;

namespace TodoSystem.API.IntegrationTests;

public class ExternalTodoApi2Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExternalTodoApi2Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace authentication with test authentication
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, ExternalTestAuthHandler>("Test", options => { });

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

    private HttpClient CreateClientWithMockedJsonPlaceholder(Func<HttpRequestMessage, HttpResponseMessage> mockResponse)
    {
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) => mockResponse(request));

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing IExternalTodoService registration
                var serviceDescriptor = services.FirstOrDefault(d =>
                    d.ServiceType == typeof(TodoSystem.Application.Services.IExternalTodoService));
                if (serviceDescriptor != null)
                {
                    services.Remove(serviceDescriptor);
                }

                // Register HttpClient with mocked handler for JsonPlaceholderService
                services.AddHttpClient<TodoSystem.Application.Services.IExternalTodoService,
                    TodoSystem.Infrastructure.ExternalServices.JsonPlaceholderService>(client =>
                {
                    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
                })
                .ConfigurePrimaryHttpMessageHandler(() => mockHandler.Object);
            });
        });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "test-token");
        return client;
    }

    [Fact]
    public async Task Get_External_Todos_Returns_Ok()
    {
        // Arrange
        var client = CreateClientWithMockedJsonPlaceholder(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.EndsWith("/todos", request.RequestUri!.AbsolutePath, StringComparison.Ordinal);

            var payload = new[]
            {
                new { id = 1, userId = 1, title = "External Todo 1", completed = false },
                new { id = 2, userId = 1, title = "External Todo 2", completed = true }
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            };
        });

        // Act
        var response = await client.GetAsync("/api/v1/externaltodos");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodoDto[]>();
        Assert.NotNull(body);
        Assert.Equal(2, body!.Length);
        Assert.Contains(body, t => t.Title == "External Todo 1");
        Assert.Contains(body, t => t.Title == "External Todo 2");
    }

    [Fact]
    public async Task Get_External_Todo_ById_Returns_Ok()
    {
        // Arrange
        var client = CreateClientWithMockedJsonPlaceholder(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.EndsWith("/todos/42", request.RequestUri!.AbsolutePath, StringComparison.Ordinal);

            var payload = new { id = 42, userId = 7, title = "External Todo 42", completed = false };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            };
        });

        // Act
        var response = await client.GetAsync("/api/v1/externaltodos/42");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(body);
        Assert.Equal("External Todo 42", body!.Title);
    }

    [Fact]
    public async Task Get_External_Todo_ById_Returns_NotFound()
    {
        // Arrange
        var client = CreateClientWithMockedJsonPlaceholder(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.EndsWith("/todos/404", request.RequestUri!.AbsolutePath, StringComparison.Ordinal);

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        // Act
        var response = await client.GetAsync("/api/v1/externaltodos/404");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Create_External_Todo_Returns_Created()
    {
        // Arrange
        var client = CreateClientWithMockedJsonPlaceholder(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.EndsWith("/todos", request.RequestUri!.AbsolutePath, StringComparison.Ordinal);

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent.Create(new { id = 101 })
            };
        });

        var command = new CreateExternalTodoCommand { Title = "New External Todo" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/externaltodos", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(body);
        Assert.Equal("New External Todo", body!.Title);
    }

    [Fact]
    public async Task Put_Update_External_Todo_Returns_BadRequest_On_Id_Mismatch()
    {
        // Arrange
        var client = CreateClientWithMockedJsonPlaceholder(request =>
        {
            // This should not be called due to controller validation
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var command = new UpdateExternalTodoCommand { ExternalId = 2, Title = "Mismatched ID" };

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/externaltodos/1", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_Update_External_Todo_Returns_Ok()
    {
        // Arrange
        var client = CreateClientWithMockedJsonPlaceholder(request =>
        {
            var path = request.RequestUri!.AbsolutePath;

            if (request.Method == HttpMethod.Get && path.EndsWith("/todos/7", StringComparison.Ordinal))
            {
                // Existence check performed by handler
                var payload = new { id = 7, userId = 1, title = "Original Title", completed = false };
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(payload) };
            }

            if (request.Method == HttpMethod.Put && path.EndsWith("/todos/7", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var command = new UpdateExternalTodoCommand { ExternalId = 7, Title = "Updated External Todo" };

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/externaltodos/7", command);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(body);
        Assert.Equal("Updated External Todo", body!.Title);
    }

    [Fact]
    public async Task Delete_External_Todo_Returns_NoContent()
    {
        // Arrange
        var client = CreateClientWithMockedJsonPlaceholder(request =>
        {
            var path = request.RequestUri!.AbsolutePath;

            if (request.Method == HttpMethod.Get && path.EndsWith("/todos/9", StringComparison.Ordinal))
            {
                // Existence check
                var payload = new { id = 9, userId = 1, title = "To be deleted", completed = false };
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(payload) };
            }

            if (request.Method == HttpMethod.Delete && path.EndsWith("/todos/9", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        // Act
        var response = await client.DeleteAsync("/api/v1/externaltodos/9");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_External_Todo_Returns_NotFound()
    {
        // Arrange
        var client = CreateClientWithMockedJsonPlaceholder(request =>
        {
            var path = request.RequestUri!.AbsolutePath;

            if (request.Method == HttpMethod.Get && path.EndsWith("/todos/10", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (request.Method == HttpMethod.Delete && path.EndsWith("/todos/10", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        // Act
        var response = await client.DeleteAsync("/api/v1/externaltodos/10");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Test authentication handler that always succeeds
    private sealed class ExternalTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public ExternalTestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user"),
                new Claim(ClaimTypes.Name, "Test User")
            }, Scheme.Name);

            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}