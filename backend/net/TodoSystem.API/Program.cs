using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using OwaspHeaders.Core.Extensions;
using OwaspHeaders.Core.Models;
using Serilog;
using Serilog.Formatting.Compact;
using System.Text;
using System.Threading.RateLimiting;
using TodoSystem.API;
using TodoSystem.API.Middleware;
using TodoSystem.Application;
using TodoSystem.Application.Todos.Commands;
using TodoSystem.Application.Todos.Queries;
using TodoSystem.Infrastructure;
using TodoSystem.Infrastructure.Data;
using TodoSystem.Infrastructure.ExternalServices;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

Log.Information("Starting up");

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Logging, should disable serilog in aspire for structured logging
// builder.Host.UseSerilog();

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

// DbContext
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddOptions<JsonPlaceholderOptions>()
//    .Bind(builder.Configuration.GetSection(JsonPlaceholderOptions.SectionName))
//.ValidateDataAnnotations()
//.Validate(config=>
//{
//    // Custom validation logic
//    return Uri.IsWellFormedUriString(config.BaseUrl, UriKind.Absolute)
//        && config.TimeoutSeconds > 0
//        && config.RetryCount >= 0
//        && config.CacheDurationMinutes >= 0;
//}, "Invalid JsonPlaceholder configuration");

// Dependency Injection
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Memory Cache
// builder.Services.AddMemoryCache(options =>
// {
//     options.SizeLimit = 1024; // Limit cache size
//     options.TrackStatistics = true; // Enable cache statistics
// });

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

// CORS policy name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add CORS services with a named policy (adjust origins as needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(
                    "https://localhost:7148" // Your dev HTTPS port
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100, // max requests per window
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// builder.Services.AddHostedService<TodoSystem.Infrastructure.Services.KafkaConsumerService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Response Compression (early in pipeline)
app.UseResponseCompression();

// Add HTTPS redirection middleware early in the pipeline
app.UseHttpsRedirection();

// Add HSTS middleware (HTTP Strict Transport Security)
app.UseHsts();

// Use OWASP Secure Headers with default recommended configuration
// See: https://github.com/GaProgMan/OwaspHeaders.Core#secure-headers
app.UseSecureHeadersMiddleware(
    SecureHeadersMiddlewareBuilder
        .CreateBuilder()
        .UseHsts() // HTTP Strict Transport Security
        .UseXFrameOptions() // Prevent clickjacking
        .UseContentTypeOptions() // Prevent MIME sniffing
        .UseContentSecurityPolicy() // Adds a default Content-Security-Policy
        .UsePermittedCrossDomainPolicies()
        .UseReferrerPolicy()
        .UseCacheControl()
        .UseXssProtection() // Adds X-XSS-Protection: 0 (modern browsers ignore, but harmless)
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

// Enable CORS middleware (must be after UseRouting and before UseAuthorization)
app.UseCors(MyAllowSpecificOrigins);

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
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                    IsEssential = true
                });
        }
    }
    await next();
});

// Enable Rate Limiting middleware
app.UseRateLimiter();

app.MapHealthChecks("/health");

// Map MVC controllers (ExternalTodosController)
app.MapControllers();
app.MapFeatureEndpoints();

app.Run();

public partial class Program { }