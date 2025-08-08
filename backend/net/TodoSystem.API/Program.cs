using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Compact;
using TodoSystem.Infrastructure.Data;
using TodoSystem.Infrastructure;
using TodoSystem.Application;
using TodoSystem.Application.Todos.Commands;
using TodoSystem.Application.Todos.Queries;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TodoSystem.API.Middleware;
using Microsoft.AspNetCore.Mvc;
using TodoSystem.Application.Auth.Commands;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter()) // Structured JSON logs
    .CreateBootstrapLogger();

Log.Information("Starting up");

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Logging, should disable serilog in aspire for structured logging
// builder.Host.UseSerilog();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// // MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateTodoCommand).Assembly));

// // JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// // Authorization
builder.Services.AddAuthorization();


// // Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TodoDbContext>();

// Configure ProblemDetails for consistent error responses
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true; // We'll handle this ourselves
});

// Add ProblemDetails for consistent error responses
builder.Services.AddProblemDetails();

// Add OpenTelemetry logging
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    // You can add resource attributes or exporters here if needed
});

//builder.Services.AddOpenTelemetry()
//    .ConfigureResource(resource => resource
//        .AddService("TodoSystem.API"))
//    .WithTracing(tracing => tracing
//        .AddAspNetCoreInstrumentation()
//        .AddHttpClientInstrumentation()
//        .AddOtlpExporter() // Exports to OTLP endpoint if OTEL_EXPORTER_OTLP_ENDPOINT is set
//    )
//    .WithMetrics(metrics => metrics
//        .AddAspNetCoreInstrumentation()
//        .AddHttpClientInstrumentation()
//        .AddRuntimeInstrumentation()
//        .AddOtlpExporter()
//    );

var app = builder.Build();

app.MapDefaultEndpoints();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add the custom exception handling middleware
app.UseCustomExceptionHandler();
app.UseRequestResponseLogging(); // <-- Add this line
//app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();


app.UseHttpsRedirection();
app.MapHealthChecks("/health");

// Minimal API endpoints
app.MapPost("/api/v1/todos", async (CreateTodoCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return Results.Created($"/api/v1/todos/{result.Id}", result);
}).RequireAuthorization();

app.MapGet("/api/v1/todos", async (IMediator mediator) =>
{
    var result = await mediator.Send(new GetTodosQuery());
    return Results.Ok(result);
}).RequireAuthorization();

// Authentication endpoint
app.MapPost("/api/v1/auth/login", async (LoginCommand command, IMediator mediator) =>
{
    try
    {
        var result = await mediator.Send(command);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "An error occurred during authentication",
            statusCode: 500,
            detail: ex.Message
        );
    }
});

// Registration endpoint
app.MapPost("/api/v1/auth/register", async (RegisterCommand command, IMediator mediator) =>
{
    try
    {
        var result = await mediator.Send(command);

        if (!result.Success)
        {
            return Results.BadRequest(new { message = result.Message });
        }

        return Results.Created($"/api/v1/auth/users/{result.Email}", result);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "An error occurred during registration",
            statusCode: 500,
            detail: ex.Message
        );
    }
});

// Get todo by id
app.MapGet("/api/v1/todos/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new GetTodoByIdQuery(id));
    return result is not null ? Results.Ok(result) : Results.NotFound();
}).RequireAuthorization();

// Update todo
app.MapPut("/api/v1/todos/{id:guid}", async (Guid id, UpdateTodoCommand command, IMediator mediator) =>
{
    if (id != command.Id)
        return Results.BadRequest(new { message = "ID in URL and body do not match." });

    var result = await mediator.Send(command);
    return Results.Ok(result);
}).RequireAuthorization();

// Delete todo
app.MapDelete("/api/v1/todos/{id:guid}", async (Guid id, IMediator mediator) =>
{
    await mediator.Send(new DeleteTodoCommand { Id = id });
    return Results.NoContent();
}).RequireAuthorization();

app.MapGet("/", () => "Hello World!");

app.Run();