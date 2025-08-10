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
using TodoSystem.API;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Todos.Commands;
using TodoSystem.Application.Todos.Queries;
using Xunit;

namespace TodoSystem.API.IntegrationTests;

public class ExternalTodoApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExternalTodoApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace IMediator with a mock per-test via IServiceScopeFactory
                // Tests will override this registration using .ConfigureServices if needed
                var existing = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (existing != null) services.Remove(existing);
                services.AddSingleton(Mock.Of<IMediator>());

                // Add a test auth scheme that always authenticates
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, ExternalTestAuthHandler>("Test", _ => { });
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

    [Fact]
    public async Task Get_External_Todos_Returns_Ok()
    {
        var client = CreateAuthenticatedClient(m =>
        {
            var todos = new[]
            {
                new TodoDto { Id = Guid.NewGuid().ToString(), Title = "External 1" },
                new TodoDto { Id = Guid.NewGuid().ToString(), Title = "External 2" }
            };
            m.Setup(x => x.Send(It.IsAny<GetExternalTodosQuery>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(todos);
        });

        var response = await client.GetAsync("/api/v1/externaltodos");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TodoDto[]>();
        Assert.NotNull(body);
        Assert.Equal(2, body!.Length);
    }

    [Fact]
    public async Task Get_External_Todo_ById_Returns_Ok()
    {
        var client = CreateAuthenticatedClient(m =>
        {
            var dto = new TodoDto { Id = Guid.NewGuid().ToString(), Title = "External 42" };
            m.Setup(x => x.Send(It.Is<GetExternalTodoByIdQuery>(q => q.Id == 42), It.IsAny<CancellationToken>()))
             .ReturnsAsync(dto);
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
        var client = CreateAuthenticatedClient(m =>
        {
            m.Setup(x => x.Send(It.Is<GetExternalTodoByIdQuery>(q => q.Id == 404), It.IsAny<CancellationToken>()))
             .ReturnsAsync((TodoDto?)null);
        });

        var response = await client.GetAsync("/api/v1/externaltodos/404");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Create_External_Todo_Returns_Created()
    {
        var client = CreateAuthenticatedClient(m =>
        {
            var created = new TodoDto { Id = Guid.NewGuid().ToString(), Title = "Created external" };
            m.Setup(x => x.Send(It.Is<CreateExternalTodoCommand>(c => c.Title == "Created external"), It.IsAny<CancellationToken>()))
             .ReturnsAsync(created);
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
        var client = CreateAuthenticatedClient(_ => { });

        var response = await client.PutAsJsonAsync("/api/v1/externaltodos/1",
            new UpdateExternalTodoCommand { ExternalId = 2, Title = "Mismatch" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_Update_External_Todo_Returns_Ok()
    {
        var client = CreateAuthenticatedClient(m =>
        {
            var updated = new TodoDto { Id = Guid.NewGuid().ToString(), Title = "Updated external" };
            m.Setup(x => x.Send(It.Is<UpdateExternalTodoCommand>(c => c.ExternalId == 7 && c.Title == "Updated external"), It.IsAny<CancellationToken>()))
             .ReturnsAsync(updated);
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
        var client = CreateAuthenticatedClient(m =>
        {
            m.Setup(x => x.Send(It.Is<DeleteExternalTodoCommand>(c => c.ExternalId == 9), It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);
        });

        var response = await client.DeleteAsync("/api/v1/externaltodos/9");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_External_Todo_Returns_NotFound()
    {
        var client = CreateAuthenticatedClient(m =>
        {
            m.Setup(x => x.Send(It.Is<DeleteExternalTodoCommand>(c => c.ExternalId == 10), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
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