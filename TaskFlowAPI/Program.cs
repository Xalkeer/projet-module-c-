using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using TaskFlowAPI.Middleware;
using TaskFlowAPI.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskFlow API",
        Version = "v1"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez un token JWT valide.",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [jwtSecurityScheme] = Array.Empty<string>()
    });
});

var connectionString = builder.Configuration.GetConnectionString("TaskFlowContext")
    ?? throw new InvalidOperationException("Connection string 'TaskFlowContext' not found.");

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("JWT issuer is missing.");
var jwtAudience = jwtSection["Audience"] ?? throw new InvalidOperationException("JWT audience is missing.");
var jwtSecretKey = jwtSection["Key"] ?? throw new InvalidOperationException("JWT key is missing.");
var jwtExpiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var parsedExpiryMinutes)
    ? parsedExpiryMinutes
    : 60;

builder.Services.AddScoped<TaskFlowContext>(_ => new TaskFlowContext(connectionString));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<IJwtTokenService>(_ => new JwtTokenService(
    jwtIssuer,
    jwtAudience,
    jwtSecretKey,
    TimeSpan.FromMinutes(jwtExpiryMinutes)));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
