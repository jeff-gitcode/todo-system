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
using TodoSystem.API;
using Microsoft.AspNetCore.Mvc;
using OwaspHeaders.Core.Extensions;

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

// Add MVC controllers
builder.Services.AddControllers();

// DbContext
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateTodoCommand).Assembly));

// JWT Authentication
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

// Authorization
builder.Services.AddAuthorization();

// Health Checks
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

// builder.Services.AddOpenTelemetry()
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

app.UseSecureHeadersMiddleware();

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

// Map MVC controllers (ExternalTodosController)
app.MapControllers();

// Use extension method to map all feature endpoints
app.MapFeatureEndpoints();

app.Run();

// In Program.cs
public partial class Program { } // This makes the auto-generated Program class public