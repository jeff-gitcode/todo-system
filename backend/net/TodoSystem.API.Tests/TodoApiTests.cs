using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TodoSystem.API;
using TodoSystem.Application.Dtos;
using TodoSystem.Application.Todos.Commands;

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
                    .ReturnsAsync(new TodoDto { Id = Guid.NewGuid(), Title = "Test" });

                services.AddSingleton(mockMediator.Object);
            });
        });
    }

    [Fact]
    public async Task Post_Todo_Returns_Created()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new CreateTodoCommand { Title = "Test" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/todos", command);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}