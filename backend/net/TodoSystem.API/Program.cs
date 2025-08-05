using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using TodoSystem.Infrastructure.Data;
using TodoSystem.Infrastructure.Repositories;
using TodoSystem.Domain.Repositories;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// DbContext
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

// MediatR
builder.Services.AddMediatR(typeof(TodoSystem.Application.Todos.Commands.CreateTodoCommand).Assembly);

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* JWT options */ });

// Authorization
builder.Services.AddAuthorization(options => { /* Role policies */ });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Version = "v1" });
    // ...JWT config...
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TodoDbContext>();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health");

// Minimal API endpoints
app.MapPost("/api/v1/todos", async (/* params, services */) =>
{
    // Example: Use MediatR to handle CreateTodoCommand
    // ...existing code...
}).RequireAuthorization();

app.MapGet("/api/v1/todos", async (/* params, services */) =>
{
    // Example: Use MediatR to handle GetTodosQuery
    // ...existing code...
}).RequireAuthorization();

// ...other endpoints...

app.Run();