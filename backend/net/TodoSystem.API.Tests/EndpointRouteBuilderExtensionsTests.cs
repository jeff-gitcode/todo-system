using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using TodoSystem.Application.Todos.Commands;
using TodoSystem.Application.Todos.Queries;
using Microsoft.AspNetCore.Builder;
using System;
using System.IO;
using TodoSystem.Application.Dtos;
using System.Collections.Generic;
using System.Linq; // Add this using if not present

namespace TodoSystem.API.Tests
{
    public class TodoEndpointsTests
    {
        private static IEndpointRouteBuilder CreateRouteBuilder(IMediator mediator)
        {
            var services = new ServiceCollection();
            services.AddSingleton(mediator);
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory, Microsoft.Extensions.Logging.LoggerFactory>(); // Add this line
            var app = new ApplicationBuilder(services.BuildServiceProvider());
            return new RouteBuilderStub(app);
        }

        [Fact]
        public async Task Post_Todo_Returns_Created()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var createdResult = new TodoDto { Id = Guid.NewGuid(), Title = "Test" };
            mediator.Setup(m => m.Send(It.IsAny<CreateTodoCommand>(), default)).ReturnsAsync(createdResult);

            var builder = CreateRouteBuilder(mediator.Object);
            TodoEndpoints.Map(builder);

            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Path = "/api/v1/todos/";
            context.RequestServices = builder.ServiceProvider;
            var commandJson = JsonSerializer.Serialize(new CreateTodoCommand { Title = "Test" });
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(commandJson));
            context.Request.ContentType = "application/json";

            // Act
            var endpoint = builder.DataSources.First().Endpoints[0];
            var del = endpoint.RequestDelegate;
            await del(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.Created, context.Response.StatusCode);
        }

        [Fact]
        public async Task Get_Todos_Returns_Ok()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var todosResult = new[] { new TodoDto { Id = Guid.NewGuid(), Title = "Test" } };
            mediator.Setup(m => m.Send(It.IsAny<GetTodosQuery>(), default)).ReturnsAsync(todosResult);

            var builder = CreateRouteBuilder(mediator.Object);
            TodoEndpoints.Map(builder);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/v1/todos/";
            context.RequestServices = builder.ServiceProvider;

            // Act
            var endpoint = builder.DataSources.First().Endpoints[1];
            var del = endpoint.RequestDelegate;
            await del(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
        }

        [Fact]
        public async Task Get_Todo_By_Id_Returns_Ok_When_Found()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var todoId = Guid.NewGuid();
            var todoResult = new TodoDto { Id = todoId, Title = "Test" };
            mediator.Setup(m => m.Send(It.Is<GetTodoByIdQuery>(q => q.Id == todoId), default)).ReturnsAsync(todoResult);

            var builder = CreateRouteBuilder(mediator.Object);
            TodoEndpoints.Map(builder);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = $"/api/v1/todos/{todoId}";

            // Setup route values correctly - the key issue is here
            var routeValues = new RouteValueDictionary();
            routeValues["id"] = todoId.ToString(); // Convert Guid to string to avoid casting issues
            context.Request.RouteValues = routeValues;

            context.RequestServices = builder.ServiceProvider;

            // Act
            var endpoint = builder.DataSources.First().Endpoints[2];
            var del = endpoint.RequestDelegate;
            await del(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
        }

        [Fact]
        public async Task Get_Todo_By_Id_Returns_NotFound_When_Not_Found()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var todoId = Guid.NewGuid();
            mediator.Setup(m => m.Send(It.Is<GetTodoByIdQuery>(q => q.Id == todoId), default)).ReturnsAsync((TodoDto?)null);

            var builder = CreateRouteBuilder(mediator.Object);
            TodoEndpoints.Map(builder);

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = $"/api/v1/todos/{todoId}";

            // Setup route values correctly - using RouteValueDictionary and string conversion
            var routeValues = new RouteValueDictionary();
            routeValues["id"] = todoId.ToString(); // Convert Guid to string to avoid casting issues
            context.Request.RouteValues = routeValues;

            context.RequestServices = builder.ServiceProvider;

            // Act
            var endpoint = builder.DataSources.First().Endpoints[2];
            var del = endpoint.RequestDelegate;
            await del(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        }

        [Fact]
        public async Task Put_Todo_Returns_Ok_When_Id_Matches()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var todoId = Guid.NewGuid();
            var updateResult = new TodoDto { Id = todoId, Title = "Updated" };
            mediator.Setup(m => m.Send(It.IsAny<UpdateTodoCommand>(), default)).ReturnsAsync(updateResult);

            var builder = CreateRouteBuilder(mediator.Object);
            TodoEndpoints.Map(builder);

            var context = new DefaultHttpContext();
            context.Request.Method = "PUT";
            context.Request.Path = $"/api/v1/todos/{todoId}";

            // Setup route values correctly
            var routeValues = new RouteValueDictionary();
            routeValues["id"] = todoId.ToString(); // Convert Guid to string to avoid casting issues
            context.Request.RouteValues = routeValues;

            context.RequestServices = builder.ServiceProvider;
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new UpdateTodoCommand { Id = todoId, Title = "Updated" })));
            context.Request.ContentType = "application/json"; // Add content type header

            // Act
            var endpoint = builder.DataSources.First().Endpoints[3];
            var del = endpoint.RequestDelegate;
            await del(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
        }

        [Fact]
        public async Task Put_Todo_Returns_BadRequest_When_Id_Mismatch()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var todoId = Guid.NewGuid();
            var otherId = Guid.NewGuid();

            var builder = CreateRouteBuilder(mediator.Object);
            TodoEndpoints.Map(builder);

            var context = new DefaultHttpContext();
            context.Request.Method = "PUT";
            context.Request.Path = $"/api/v1/todos/{todoId}";

            // Setup route values correctly
            var routeValues = new RouteValueDictionary();
            routeValues["id"] = todoId.ToString(); // Convert Guid to string to avoid casting issues
            context.Request.RouteValues = routeValues;

            context.RequestServices = builder.ServiceProvider;
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new UpdateTodoCommand { Id = otherId, Title = "Updated" })));
            context.Request.ContentType = "application/json"; // Add content type header

            // Act
            var endpoint = builder.DataSources.First().Endpoints[3];
            var del = endpoint.RequestDelegate;
            await del(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task Delete_Todo_Returns_NoContent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var todoId = Guid.NewGuid();
            mediator.Setup(m => m.Send(It.IsAny<DeleteTodoCommand>(), default)).Returns(Task.CompletedTask);

            var builder = CreateRouteBuilder(mediator.Object);
            TodoEndpoints.Map(builder);

            var context = new DefaultHttpContext();
            context.Request.Method = "DELETE";
            context.Request.Path = $"/api/v1/todos/{todoId}";

            // Setup route values correctly
            var routeValues = new RouteValueDictionary();
            routeValues["id"] = todoId.ToString(); // Convert Guid to string to avoid casting issues
            context.Request.RouteValues = routeValues;

            context.RequestServices = builder.ServiceProvider;

            // Act
            var endpoint = builder.DataSources.First().Endpoints[4];
            var del = endpoint.RequestDelegate;
            await del(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.NoContent, context.Response.StatusCode);
        }

        // Helper stub for IEndpointRouteBuilder
        private class RouteBuilderStub : IEndpointRouteBuilder
        {
            public IServiceProvider ServiceProvider { get; }
            public ICollection<EndpointDataSource> DataSources { get; } = new List<EndpointDataSource>();
            public IApplicationBuilder CreateApplicationBuilder() => new ApplicationBuilder(ServiceProvider);
            public RouteBuilderStub(IApplicationBuilder app) => ServiceProvider = app.ApplicationServices;
        }
    }
}