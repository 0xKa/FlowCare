using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();


var app = builder.Build();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
