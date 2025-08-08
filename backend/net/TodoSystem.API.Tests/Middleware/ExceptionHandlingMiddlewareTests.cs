using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TodoSystem.API.Middleware;
using System.Collections.Generic;

namespace TodoSystem.API.Middleware.Tests
{
    public class ExceptionHandlingMiddlewareTest
    {
        private DefaultHttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            return context;
        }

        [Fact]
        public async Task InvokeAsync_Should_Call_Next_When_No_Exception()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
            var env = Mock.Of<IWebHostEnvironment>(e => e.EnvironmentName == "Development");
            var wasCalled = false;
            RequestDelegate next = ctx =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            };
            var middleware = new ExceptionHandlingMiddleware(next, logger, env);
            var context = CreateHttpContext();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(wasCalled);
        }

        [Fact]
        public async Task InvokeAsync_Should_Return_Validation_ProblemDetails_On_ValidationException()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
            var env = Mock.Of<IWebHostEnvironment>(e => e.EnvironmentName == "Development");
            RequestDelegate next = ctx => throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Title", "Title is required")
            });
            var middleware = new ExceptionHandlingMiddleware(next, logger, env);
            var context = CreateHttpContext();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = new StreamReader(context.Response.Body).ReadToEnd();
            Assert.Contains("Validation error", response);
            Assert.Contains("Title is required", response);
            Assert.Equal("application/problem+json", context.Response.ContentType);
            Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Should_Return_NotFound_ProblemDetails_On_KeyNotFoundException()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
            var env = Mock.Of<IWebHostEnvironment>(e => e.EnvironmentName == "Development");
            RequestDelegate next = ctx => throw new KeyNotFoundException("User not found");
            var middleware = new ExceptionHandlingMiddleware(next, logger, env);
            var context = CreateHttpContext();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = new StreamReader(context.Response.Body).ReadToEnd();
            Assert.Contains("Resource not found", response);
            Assert.Contains("User not found", response);
            Assert.Equal("application/problem+json", context.Response.ContentType);
            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Should_Return_InternalServerError_ProblemDetails_On_UnknownException_Production()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
            var env = Mock.Of<IWebHostEnvironment>(e => e.EnvironmentName == "Production");
            RequestDelegate next = ctx => throw new Exception("Something went wrong");
            var middleware = new ExceptionHandlingMiddleware(next, logger, env);
            var context = CreateHttpContext();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = new StreamReader(context.Response.Body).ReadToEnd();
            Assert.Contains("An unexpected error occurred", response);
            Assert.DoesNotContain("Something went wrong", response); // Should not leak details in production
            Assert.Equal("application/problem+json", context.Response.ContentType);
            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Should_Return_InternalServerError_ProblemDetails_On_UnknownException_Development()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
            var env = Mock.Of<IWebHostEnvironment>(e => e.EnvironmentName == "Development");
            RequestDelegate next = ctx => throw new Exception("Dev error details");
            var middleware = new ExceptionHandlingMiddleware(next, logger, env);
            var context = CreateHttpContext();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = new StreamReader(context.Response.Body).ReadToEnd();
            Assert.Contains("An unexpected error occurred", response);
            Assert.Contains("Dev error details", response); // Should show details in development
            Assert.Equal("application/problem+json", context.Response.ContentType);
            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        }
    }
}