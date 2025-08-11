using MediatR;
using TodoSystem.Application.Todos.Commands;
using TodoSystem.Application.Todos.Queries;
using TodoSystem.Application.Auth.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery; // Add this at the top
using System.Text.Encodings.Web; // Add for encoding if needed

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        TodoEndpoints.Map(app);
        AuthEndpoints.Map(app);
        // Disable external todos for minimal, use controller instead
        // ExternalTodoEndpoints.Map(app);

        return app;
    }
}

public class AuthEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth");
        group.MapPost("/login", async ([FromBody] LoginCommand command, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(command);
                // No HTML returned, only JSON, so XSS risk is minimized
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                // Defensive: encode error details if ever returned to client
                return Results.Problem(
                    title: "An error occurred during authentication",
                    statusCode: 500,
                    detail: JavaScriptEncoder.Default.Encode(ex.Message)
                );
            }
        });

        group.MapPost("/register", async ([FromBody] RegisterCommand command, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(command);

                if (!result.Success)
                {
                    // Defensive: encode message if ever returned to client
                    return Results.BadRequest(new { message = JavaScriptEncoder.Default.Encode(result.Message) });
                }

                return Results.Created($"/api/v1/auth/users/{result.Email}", result);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "An error occurred during registration",
                    statusCode: 500,
                    detail: JavaScriptEncoder.Default.Encode(ex.Message)
                );
            }
        });
    }
}


public class TodoEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/todos");

        // Require antiforgery token for unsafe methods (POST, PUT, DELETE)
        group.MapPost("/", async ([FromBody] CreateTodoCommand command, IMediator mediator, IAntiforgery antiforgery, HttpContext httpContext) =>
        {
            // Validate antiforgery token
            await antiforgery.ValidateRequestAsync(httpContext);

            var result = await mediator.Send(command);
            return Results.Created($"/api/v1/todos/{result.Id}", result);
        }).RequireAuthorization();

        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTodosQuery());
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTodoByIdQuery(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        }).RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateTodoCommand command, IMediator mediator, IAntiforgery antiforgery, HttpContext httpContext) =>
        {
            // Validate antiforgery token
            await antiforgery.ValidateRequestAsync(httpContext);

            if (id != command.Id)
                return Results.BadRequest(new { message = "ID in URL and body do not match." });

            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, IAntiforgery antiforgery, HttpContext httpContext) =>
        {
            // Validate antiforgery token
            await antiforgery.ValidateRequestAsync(httpContext);

            await mediator.Send(new DeleteTodoCommand { Id = id });
            return Results.NoContent();
        }).RequireAuthorization();
    }
}

public class ExternalTodoEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/externaltodos");

        // ... existing local todo endpoints ...

        // External todos endpoints
        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetExternalTodosQuery());
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/{id:int}", async (int id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetExternalTodoByIdQuery(id));
            return result != null ? Results.Ok(result) : Results.NotFound();
        }).RequireAuthorization();

        group.MapPost("/", async ([FromBody] CreateExternalTodoCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/v1/todos/external", result);
        }).RequireAuthorization();

        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateExternalTodoCommand command, IMediator mediator) =>
        {
            if (id != command.ExternalId)
                return Results.BadRequest(new { message = "ID in URL and body do not match." });

            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapDelete("/{id:int}", async (int id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteExternalTodoCommand { ExternalId = id });
            return result ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();
    }
}