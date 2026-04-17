using DAL;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("TaskFlowContext")
    ?? throw new InvalidOperationException("Connection string 'TaskFlowContext' not found.");

builder.Services.AddScoped<TaskFlowContext>(_ => new TaskFlowContext(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<TaskFlowContext>();
        var created = context.Database.EnsureCreated();

        logger.LogInformation(created
            ? "Base de donnees creee et schema initialise."
            : "Base de donnees deja presente, schema verifie.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Echec de l'initialisation de la base de donnees.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
