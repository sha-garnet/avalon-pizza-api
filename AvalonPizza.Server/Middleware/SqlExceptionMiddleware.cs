using Microsoft.Data.SqlClient;
using System.Net;
using System.Text.Json;

namespace AvalonPizza.Server.Middleware
{
    public class SqlExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SqlExceptionMiddleware> _logger;

        public SqlExceptionMiddleware(
            RequestDelegate next, 
            ILogger<SqlExceptionMiddleware> logger)
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
            catch (SqlException ex)
            {
                _logger.LogError(ex, "A database error occurred.");
                await HandleSqlExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled server error occurred.");
                await HandleGeneralExceptionAsync(context, ex);
            }
        }

        private static Task HandleSqlExceptionAsync(HttpContext context, SqlException ex)
        {
            context.Response.ContentType = "application/json";

            // Default to 500, then narrow it down based on your "State Dictionary"
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "A database error occurred.";

            // SQL State mapping logic
            switch (ex.State)
            {
                case 1: // Not Found / Inactive
                    statusCode = HttpStatusCode.NotFound;
                    message = ex.Message;
                    break;
                case 2: // Business Rule (Not Pending)
                    statusCode = HttpStatusCode.UnprocessableEntity; // 422
                    message = ex.Message;
                    break;
                case 3: // Invalid Status ID
                case 4: // Empty Toppings
                case 5: // Bad Topping IDs
                    statusCode = HttpStatusCode.BadRequest;
                    message = ex.Message;
                    break;
                default:
                    message = "An unexpected database constraint was violated.";
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new
            {
                error = message,
                sqlState = ex.State,
                code = statusCode
            });

            return context.Response.WriteAsync(result);
        }

        private static Task HandleGeneralExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "A critical server error occurred."
            }));
        }
    }
}