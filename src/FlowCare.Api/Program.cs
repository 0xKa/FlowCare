using FlowCare.Api.Startup;
using FlowCare.Application.Interfaces;
using FlowCare.Infrastructure;
using FlowCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddCustomApiBehavior();
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
            var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
            var seeder = new SeedDataImporter(db, auditLogService);
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

var enableApiDocsInProduction = builder.Configuration.GetValue<bool>("ApiDocs:EnableInProduction");
if (!app.Environment.IsDevelopment() && enableApiDocsInProduction)
{
    app.MapOpenApi();
    app.MapScalarApiReference(
        options =>
        {
            options.Title = "FlowCare API Reference";
            options.Theme = ScalarTheme.Kepler;
            options.HideClientButton = true;
            options.ExpandAllResponses = true;
            options.Agent = new ScalarAgentOptions
            {
                Disabled = true
            };
        }
    );
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
