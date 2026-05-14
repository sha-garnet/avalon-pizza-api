using Microsoft.AspNetCore.Authorization;

namespace AvalonPizza.Server.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "X-Api-Key";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Get the endpoint that routing identified
            var endpoint = context.GetEndpoint();
            // 2. Check if the controller or action has the [Authorize] attribute
            var authorizeAttribute = endpoint?.Metadata.GetMetadata<IAuthorizeData>();

            // If it's public (no [Authorize]), just move on
            // 3. If there is NO [Authorize] attribute, let the request through (Public access)
            if (authorizeAttribute == null)
            {
                await _next(context);
                return;
            }

            // check if header exists
            // 4. If [Authorize] IS present, perform the API Key check
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                await ReturnUnauthorized(context, "API Key was not provided.");
                return;
            }

            var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = appSettings.GetValue<string>("ApiKeySettings:ApiKey");

            // validate key
            if (apiKey == null || !apiKey.Equals(extractedApiKey))
            {
                await ReturnUnauthorized(context, "Unauthorized client.");
                return;
            }

            // All good! Move to the Controller
            await _next(context);
        }

        private static async Task ReturnUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = message, code = 401 });
        }
    }
}