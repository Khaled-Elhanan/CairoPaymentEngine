using CairoPaymentEngine.Domain.Exceptioins;
using System.Net;
using System.Text.Json;

namespace CairoPaymentEngine.Api.Middleware
{
 
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                // 404 — Not Found
                OrderNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
                PaymentNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),

                // 400 — Bad Request
                OrderNotPayableException ex => (HttpStatusCode.BadRequest, ex.Message),
                PaymentVerificationFailedException ex => (HttpStatusCode.BadRequest, ex.Message),
                GatewayNotSupportedException ex => (HttpStatusCode.BadRequest, ex.Message),
                ArgumentException ex => (HttpStatusCode.BadRequest, ex.Message),
                InvalidOperationException ex => (HttpStatusCode.BadRequest, ex.Message),

                // 500 
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            var response = JsonSerializer.Serialize(new
            {
                Error = message,
                StatusCode = (int)statusCode
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(response); 

        }
    }
}