using Microsoft.EntityFrameworkCore;
using Serilog;
using TodoSystem.Infrastructure.Data;
using TodoSystem.Infrastructure;
using TodoSystem.Application;
using TodoSystem.Application.Todos.Commands;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MediatR;
using TodoSystem.Application.Todos.Queries;
using Microsoft.Extensions.DependencyInjection;
using TodoSystem.API.Middleware;
using Microsoft.AspNetCore.Mvc;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog();

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

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add the custom exception handling middleware
app.UseCustomExceptionHandler();

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();


app.UseHttpsRedirection();
app.MapHealthChecks("/health");

// Minimal API endpoints
app.MapPost("/api/v1/todos", async (CreateTodoCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return Results.Created($"/api/v1/todos/{result.Id}", result);
});
// .RequireAuthorization();

app.MapGet("/api/v1/todos", async (IMediator mediator) =>
{
    var result = await mediator.Send(new GetTodosQuery());
    return Results.Ok(result);
});
// .RequireAuthorization();

// ...other endpoints...

app.MapGet("/", () => "Hello World!");

app.Run();