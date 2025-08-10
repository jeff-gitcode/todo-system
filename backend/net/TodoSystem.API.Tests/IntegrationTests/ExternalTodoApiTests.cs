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

public class ExternalTodoApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<HttpMessageHandler> _jsonPlaceholderHandler = new(MockBehavior.Strict);

    public ExternalTodoApiTests(WebApplicationFactory<Program> factory)
    {
        // Configure default mocked responses for JSONPlaceholder for all tests
        _jsonPlaceholderHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage r, CancellationToken _) =>
            {
                var path = r.RequestUri!.AbsolutePath;
                if (r.Method == HttpMethod.Get && path.EndsWith("/todos", StringComparison.Ordinal))
                {
                    var payload = new[]
                    {
                        new { id = 1, userId = 1, title = "from-default-mock-1", completed = false },
                        new { id = 2, userId = 1, title = "from-default-mock-2", completed = true }
                    };
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(payload) };
                }
                if (r.Method == HttpMethod.Get && path.Contains("/todos/", StringComparison.Ordinal))
                {
                    var idSegment = path.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
                    var id = int.TryParse(idSegment, out var i) ? i : 0;
                    var payload = new { id, userId = 1, title = $"from-default-mock-id-{id}", completed = false };
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(payload) };
                }
                if (r.Method == HttpMethod.Post && path.EndsWith("/todos", StringComparison.Ordinal))
                {
                    return new HttpResponseMessage(HttpStatusCode.Created) { Content = JsonContent.Create(new { id = 101 }) };
                }
                if (r.Method == HttpMethod.Put && path.Contains("/todos/", StringComparison.Ordinal))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                if (r.Method == HttpMethod.Delete && path.Contains("/todos/", StringComparison.Ordinal))
                {
                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                }

                // Default: OK empty
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace IMediator with a mock placeholder; per-test overrides will swap it
                var existing = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (existing != null) services.Remove(existing);
                services.AddSingleton(Mock.Of<IMediator>());

                // Test auth that always authenticates
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, ExternalTestAuthHandler>("Test", _ => { });

                // Ensure typed client for JsonPlaceholder uses the mocked handler for ALL tests
                var toRemove = services.Where(d => d.ServiceType == typeof(TodoSystem.Application.Services.IExternalTodoService)).ToList();
                foreach (var d in toRemove) services.Remove(d);

                services.AddHttpClient<TodoSystem.Application.Services.IExternalTodoService, TodoSystem.Infrastructure.ExternalServices.JsonPlaceholderService>()
                        .ConfigurePrimaryHttpMessageHandler(() => _jsonPlaceholderHandler.Object)
                        .ConfigureHttpClient(c =>
                        {
                            c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
                        });
            });
        });
    }

    private HttpClient CreateAuthenticatedClient(Action<Mock<IMediator>> setupMediator)
    {
        var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
        setupMediator(mockMediator);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var existing = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (existing != null) services.Remove(existing);
                services.AddSingleton(mockMediator.Object);
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "test-token");
        return client;
    }

    // Creates a client with mocked outbound HTTP for JSONPlaceholder,
    // real MediatR pipeline, and test auth.
    private static HttpClient CreateClientWithMockedJsonPlaceholder(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage r, CancellationToken _) => responder(r));

        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Test auth that always succeeds
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, ExternalTestAuthHandler>("Test", _ => { });

                // Replace typed client for IExternalTodoService with mocked handler
                var toRemove = services.Where(d => d.ServiceType == typeof(TodoSystem.Application.Services.IExternalTodoService)).ToList();
                foreach (var d in toRemove) services.Remove(d);

                services.AddHttpClient<TodoSystem.Application.Services.IExternalTodoService, TodoSystem.Infrastructure.ExternalServices.JsonPlaceholderService>()
                        .ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object)
                        .ConfigureHttpClient(c =>
                        {
                            c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
                        });
            });
        });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "test-token");
        return client;
    }

    [Fact]
    public async Task Get_External_Todos_Returns_Ok()
    {
        var client = CreateClientWithMockedJsonPlaceholder(req =>
        {
            Assert.EndsWith("/todos", req.RequestUri!.AbsolutePath, StringComparison.Ordinal);
            var payload = new[]
            {
                new { id = 1, userId = 1, title = "from-mock-1", completed = false },
                new { id = 2, userId = 1, title = "from-mock-2", completed = true }
            };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            };
        });

        var response = await client.GetAsync("/api/v1/externaltodos");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodoDto[]>();
        Assert.NotNull(body);
        Assert.Equal(2, body!.Length);
        Assert.Contains(body, t => t.Title == "from-mock-1");
        Assert.Contains(body, t => t.Title == "from-mock-2");
    }

    [Fact]
    public async Task Get_External_Todo_ById_Returns_Ok()
    {
        var client = CreateClientWithMockedJsonPlaceholder(req =>
        {
            Assert.EndsWith("/todos/42", req.RequestUri!.AbsolutePath, StringComparison.Ordinal);
            var payload = new { id = 42, userId = 7, title = "External 42", completed = false };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload)
            };
        });

        var response = await client.GetAsync("/api/v1/externaltodos/42");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(body);
        Assert.Equal("External 42", body!.Title);
    }

    [Fact]
    public async Task Get_External_Todo_ById_Returns_NotFound()
    {
        var client = CreateClientWithMockedJsonPlaceholder(req =>
        {
            Assert.EndsWith("/todos/404", req.RequestUri!.AbsolutePath, StringComparison.Ordinal);
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var response = await client.GetAsync("/api/v1/externaltodos/404");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Create_External_Todo_Returns_Created()
    {
        var client = CreateClientWithMockedJsonPlaceholder(req =>
        {
            Assert.True(req.Method == HttpMethod.Post);
            Assert.EndsWith("/todos", req.RequestUri!.AbsolutePath, StringComparison.Ordinal);
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent.Create(new { id = 101 })
            };
        });

        var response = await client.PostAsJsonAsync("/api/v1/externaltodos", new CreateExternalTodoCommand { Title = "Created external" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(body);
        Assert.Equal("Created external", body!.Title);
    }

    [Fact]
    public async Task Put_Update_External_Todo_Returns_BadRequest_On_Id_Mismatch()
    {
        var client = CreateClientWithMockedJsonPlaceholder(_ =>
        {
            // This request should not be sent due to controller validation
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var response = await client.PutAsJsonAsync("/api/v1/externaltodos/1",
            new UpdateExternalTodoCommand { ExternalId = 2, Title = "Mismatch" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_Update_External_Todo_Returns_Ok()
    {
        var client = CreateClientWithMockedJsonPlaceholder(req =>
        {
            var path = req.RequestUri!.AbsolutePath;
            if (req.Method == HttpMethod.Get && path.EndsWith("/todos/7", StringComparison.Ordinal))
            {
                // Existence check performed by handler
                var payload = new { id = 7, userId = 1, title = "any", completed = false };
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(payload) };
            }
            if (req.Method == HttpMethod.Put && path.EndsWith("/todos/7", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var response = await client.PutAsJsonAsync("/api/v1/externaltodos/7",
            new UpdateExternalTodoCommand { ExternalId = 7, Title = "Updated external" });

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(body);
        Assert.Equal("Updated external", body!.Title);
    }

    [Fact]
    public async Task Delete_External_Todo_Returns_NoContent()
    {
        var client = CreateClientWithMockedJsonPlaceholder(req =>
        {
            var path = req.RequestUri!.AbsolutePath;
            if (req.Method == HttpMethod.Get && path.EndsWith("/todos/9", StringComparison.Ordinal))
            {
                var payload = new { id = 9, userId = 1, title = "any", completed = false };
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(payload) };
            }
            if (req.Method == HttpMethod.Delete && path.EndsWith("/todos/9", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var response = await client.DeleteAsync("/api/v1/externaltodos/9");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_External_Todo_Returns_NotFound()
    {
        var client = CreateClientWithMockedJsonPlaceholder(req =>
        {
            var path = req.RequestUri!.AbsolutePath;
            if (req.Method == HttpMethod.Get && path.EndsWith("/todos/10", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            // Delete won't be called if existence check fails, but handle anyway
            if (req.Method == HttpMethod.Delete && path.EndsWith("/todos/10", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var response = await client.DeleteAsync("/api/v1/externaltodos/10");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Simple auth handler that always authenticates as a test user
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