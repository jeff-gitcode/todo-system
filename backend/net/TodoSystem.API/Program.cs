using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
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
using OwaspHeaders.Core.Models;
using Microsoft.Net.Http.Headers;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

Log.Information("Starting up");

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MVC controllers with JSON optimization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Memory Cache
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Limit cache size
    options.TrackStatistics = true; // Enable cache statistics
});

// Response Caching
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 64 * 1024; // 64KB max body size for caching
    options.UseCaseSensitivePaths = false;
});

// Output Cache (ASP.NET Core 7+)
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("ExternalTodos", builder =>
        builder.Expire(TimeSpan.FromSeconds(30))
               .Tag("external-todos")
               .SetVaryByQuery("page", "size"));

    options.AddPolicy("UserTodos", builder =>
        builder.Expire(TimeSpan.FromSeconds(60))
               .Tag("user-todos")
               .SetVaryByQuery("filter", "sort"));

    options.AddPolicy("PublicData", builder =>
        builder.Expire(TimeSpan.FromMinutes(5))
               .Tag("public"));
});

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Add Antiforgery for CSRF protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "__Host-Csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    options.SuppressXFrameOptionsHeader = false;
});

// DbContext with connection pooling
builder.Services.AddDbContextPool<TodoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
    poolSize: 128);

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
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddProblemDetails();

// Add OpenTelemetry logging
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
});

// CORS policy
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://localhost:7148")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Response Compression (early in pipeline)
app.UseResponseCompression();

// Use OWASP Secure Headers
app.UseSecureHeadersMiddleware(
    SecureHeadersMiddlewareBuilder
        .CreateBuilder()
        .UseHsts()
        .UseXFrameOptions()
        .UseContentTypeOptions()
        .UseContentSecurityPolicy()
        .UsePermittedCrossDomainPolicies()
        .UseReferrerPolicy()
        .UseCacheControl()
        .UseXssProtection()
        .UseCrossOriginResourcePolicy()
        .Build()
);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Response Caching
app.UseResponseCaching();

// Output Cache
app.UseOutputCache();

app.UseCustomExceptionHandler();
app.UseRequestResponseLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(MyAllowSpecificOrigins);
app.UseAntiforgery();

// Emit XSRF token
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
                    HttpOnly = false,
                    Secure = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                    IsEssential = true
                });
        }
    }
    await next();
});

app.UseHttpsRedirection();
app.MapHealthChecks("/health");

// Map controllers with output caching
app.MapControllers();
app.MapFeatureEndpoints();

app.Run();

public partial class Program { }