using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TodoSystem.API;
using TodoSystem.Application.Auth.Commands;
using Xunit;

namespace TodoSystem.API.IntegrationTests;

public class AuthApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthApiTest(WebApplicationFactory<Program> factory)
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

                // Login command mocks
                mockMediator.Setup(m => m.Send(It.Is<LoginCommand>(c => c.Email == "valid@example.com"), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new LoginResponse
                    {
                        Token = "valid-jwt-token",
                        RefreshToken = "refresh-token",
                        Email = "valid@example.com",
                        DisplayName = "Valid User",
                        Expiration = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
                    });

                mockMediator.Setup(m => m.Send(It.Is<LoginCommand>(c => c.Email != "valid@example.com"), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

                // Register command mocks
                mockMediator.Setup(m => m.Send(It.Is<RegisterCommand>(c => c.Email == "exists@example.com"), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new RegisterResponse
                    {
                        Success = false,
                        Message = "Email already exists"
                    });

                mockMediator.Setup(m => m.Send(It.Is<RegisterCommand>(c => c.Email != "exists@example.com"), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new RegisterResponse
                    {
                        Success = true,
                        Email = "new@example.com",
                        DisplayName = "New User",
                        Message = "Registration successful"
                    });

                services.AddSingleton(mockMediator.Object);

                // No need for test authentication for auth endpoints since they're public
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes("Test")
                        .Build();
                });
            });
        });
    }

    [Fact]
    public async Task Login_Returns_Ok_With_Valid_Credentials()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new LoginCommand
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", command);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(result);
        Assert.Equal("valid-jwt-token", result.Token);
        Assert.Equal("valid@example.com", result.Email);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task Login_Returns_Unauthorized_With_Invalid_Credentials()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new LoginCommand
        {
            Email = "invalid@example.com",
            Password = "InvalidPassword123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", command);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_Returns_Created_With_New_Email()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new RegisterCommand
        {
            Email = "new@example.com",
            Password = "NewPassword123!",
            DisplayName = "New User"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", command);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("new@example.com", result.Email);
    }

    [Fact]
    public async Task Register_Returns_BadRequest_With_Existing_Email()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new RegisterCommand
        {
            Email = "exists@example.com",
            Password = "ExistsPassword123!",
            DisplayName = "Existing User"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BadRequestResponse>();
        Assert.NotNull(result);
        Assert.Equal("Email already exists", result.Message);
    }

    private class BadRequestResponse
    {
        public string Message { get; set; }
    }
}