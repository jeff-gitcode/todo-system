using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;
using TodoSystem.API.Middleware;

namespace TodoSystem.API.Middleware.Tests
{
    public class RequestResponseLoggingMiddlewareTest
    {
        [Fact]
        public async Task InvokeAsync_Logs_Request_And_Response()
        {
            // Arrange
            var logCalled = false;
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Sink(new DelegateSink((evt, _) =>
                {
                    logCalled = true;
                }))
                .CreateLogger();

            var context = new DefaultHttpContext();
            var requestBody = "test request";
            var responseBody = "test response";

            // Setup request body
            var requestBytes = Encoding.UTF8.GetBytes(requestBody);
            context.Request.Body = new MemoryStream(requestBytes);
            context.Request.ContentLength = requestBytes.Length;
            context.Request.Method = "POST";
            context.Request.Path = "/test";

            // Setup response
            RequestDelegate next = async ctx =>
            {
                ctx.Response.Body = new MemoryStream();
                var bytes = Encoding.UTF8.GetBytes(responseBody);
                await ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                ctx.Response.Body.Seek(0, SeekOrigin.Begin);
            };

            var middleware = new RequestResponseLoggingMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(logCalled);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var actualResponse = await new StreamReader(context.Response.Body).ReadToEndAsync();
            Assert.Equal(responseBody, actualResponse);
        }

        [Fact]
        public async Task InvokeAsync_Logs_Empty_Request_Body_If_No_Content()
        {
            // Arrange
            var logCalled = false;
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Sink(new DelegateSink((evt, _) =>
                {
                    logCalled = true;
                }))
                .CreateLogger();

            var context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream();
            context.Request.ContentLength = 0;
            context.Request.Method = "GET";
            context.Request.Path = "/empty";

            RequestDelegate next = ctx =>
            {
                ctx.Response.Body = new MemoryStream();
                return Task.CompletedTask;
            };

            var middleware = new RequestResponseLoggingMiddleware(next);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(logCalled);
        }

        private class DelegateSink : Serilog.Core.ILogEventSink
        {
            private readonly Action<Serilog.Events.LogEvent, IFormatProvider> _write;
            public DelegateSink(Action<Serilog.Events.LogEvent, IFormatProvider> write) => _write = write;
            public void Emit(Serilog.Events.LogEvent logEvent) => _write(logEvent, null);
        }
    }
}