using SimitConsulta.API.Extensions;
using SimitConsulta.API.Middleware;
using SimitConsulta.Infrastructure.DependencyInjection;
using SimitConsulta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// ── Composition Root ──────────────────────────────────────
// Único lugar que conoce todas las capas.
var builder = WebApplication.CreateBuilder(args);

// ── Registro de servicios ─────────────────────────────────

// Un método registra Domain, Application e Infrastructure completos
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig();

// CORS — permite llamadas desde el frontend Next.js
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()));

var app = builder.Build();

// ── Migraciones automáticas ───────────────────────────────
// Docker crea la BD vacía.
// EF Core aplica aquí las migraciones al arrancar.
// Con retry: SQL Server puede tardar unos segundos en estar listo.
await ApplyMigrationsAsync(app);

// ── Pipeline HTTP ─────────────────────────────────────────

// Primero — captura todas las excepciones no controladas
app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger solo en Development
if (app.Environment.IsDevelopment())
    app.UseSwaggerConfig();

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// ── Migraciones con retry ─────────────────────────────────

/// <summary>
/// Aplica migraciones EF Core con reintentos.
/// Necesario porque SQL Server puede tardar aunque
/// el healthcheck de Docker ya pasó.
/// MigrateAsync() es idempotente — no hace nada si las tablas existen.
/// </summary>
static async Task ApplyMigrationsAsync(WebApplication app)
{
    const int maxAttempts = 5;
    const int retrySeconds = 3;

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider
                      .GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider
                      .GetRequiredService<ILogger<Program>>();

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            logger.LogInformation(
                "Applying EF Core migrations " +
                "(attempt {Attempt}/{Max})...",
                attempt, maxAttempts);

            await db.Database.MigrateAsync();

            logger.LogInformation(
                "Migrations applied successfully.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(
                "Migration attempt {Attempt} failed: {Msg}. " +
                "Retrying in {Seconds}s...",
                attempt, ex.Message, retrySeconds);

            await Task.Delay(
                TimeSpan.FromSeconds(retrySeconds));
        }
    }

    logger.LogCritical(
        "Could not apply migrations after {Max} attempts.",
        maxAttempts);

    throw new InvalidOperationException(
        "Critical failure applying migrations.");
}

// Necesario para WebApplicationFactory en integration tests
public partial class Program { }