using Microsoft.AspNetCore.Antiforgery; // Add this
using Microsoft.AspNetCore.Http;        // Add this for CookieOptions
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

// Add Antiforgery for CSRF protection (see #fetch)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN"; // Angular/SPA convention
    options.Cookie.Name = "__Host-Csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.SuppressXFrameOptionsHeader = false;
});

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
app.UseRequestResponseLogging();
app.UseAuthentication();
app.UseAuthorization();

// Antiforgery middleware (must run after auth)
app.UseAntiforgery();

// Emit a JS-readable XSRF token cookie on safe GETs for SPA clients
var antiforgery = app.Services.GetRequiredService<IAntiforgery>();
app.Use(async (context, next) =>
{
    if (HttpMethods.IsGet(context.Request.Method))
    {
        var tokens = antiforgery.GetAndStoreTokens(context);
        if (!string.IsNullOrEmpty(tokens.RequestToken))
        {
            context.Response.Cookies.Append(
                "XSRF-TOKEN",
                tokens.RequestToken!,
                new CookieOptions
                {
                    HttpOnly = false, // JS readable for SPA to send in header
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    IsEssential = true
                });
        }
    }
    await next();
});

app.UseHttpsRedirection();
app.MapHealthChecks("/health");

// Map MVC controllers (ExternalTodosController)
app.MapControllers();
app.MapFeatureEndpoints();

app.Run();

public partial class Program { }