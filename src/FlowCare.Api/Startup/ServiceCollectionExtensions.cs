using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Startup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Invalid request."
                        : e.ErrorMessage)
                    .Distinct()
                    .ToArray();

                var message = errors.Length > 0
                    ? string.Join("; ", errors)
                    : "Validation failed.";

                return new BadRequestObjectResult(new { error = message });
            };
        });

        return services;
    }
}
