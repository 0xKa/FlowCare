namespace FlowCare.Api.Startup;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    IWebHostEnvironment environment,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while processing request {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var message = environment.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred.";

            await context.Response.WriteAsJsonAsync(new { error = message });
        }
    }
}
