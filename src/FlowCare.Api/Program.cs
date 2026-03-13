using FlowCare.Infrastructure;
using FlowCare.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
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
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply pending migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlowCareDbContext>();
    await db.Database.MigrateAsync();

    var seedPath = builder.Configuration["SeedDataPath"];
    if (!string.IsNullOrEmpty(seedPath))
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", seedPath);
        if (File.Exists(fullPath))
        {
            var seeder = new SeedDataImporter(db);
            await seeder.ImportAsync(fullPath);
        }
        else
            app.Logger.LogWarning("Seed data file not found at {Path}", fullPath);
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(
        options =>
        {
            options.Title = "FlowCare API Reference";
            options.Theme = ScalarTheme.DeepSpace;
            options.HideClientButton = true;
            options.ExpandAllResponses = true;
            options.Agent = new ScalarAgentOptions
            {
                Disabled = true
            };
        }
    );
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var message = app.Environment.IsDevelopment()
            ? exceptionFeature?.Error.Message ?? "An unexpected error occurred."
            : "An unexpected error occurred.";

        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
