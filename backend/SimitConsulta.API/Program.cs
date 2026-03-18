using SimitConsulta.API.Extensions;
using SimitConsulta.API.Middleware;
using SimitConsulta.Infrastructure.DependencyInjection;
using SimitConsulta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// ── Composition Root ──────────────────────────────────────
// Único lugar que conoce todas las capas.
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(opts =>
{
    opts.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(3);
    opts.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(3);
});

// ── Registro de servicios ─────────────────────────────────

// Un método registra Domain, Application e Infrastructure completos
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig();

// CORS — permite llamadas desde el frontend Next.js
builder.Services.AddCors(opts =>
    opts.AddPolicy("frontend", policy =>        // ← nombre explícito
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
// Solo aplicar migraciones si NO es entorno de testing
if (!app.Environment.IsEnvironment("Testing"))
{
    await ApplyMigrationsAsync(app);
}

// ── Pipeline HTTP ─────────────────────────────────────────
app.UseCors("frontend");
// Primero — captura todas las excepciones no controladas
app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger solo en Development
if (app.Environment.IsDevelopment())
    app.UseSwaggerConfig();


if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// ── Migraciones con retry ─────────────────────────────────

/// <summary>
/// Aplica migraciones EF Core con reintentos.
/// Necesario porque SQL Server en Docker puede tardar
/// aunque el healthcheck ya pasó.
/// CanConnectAsync() verifica la conexión antes de migrar.
/// MigrateAsync() es idempotente — no hace nada si las tablas existen.
/// </summary>
static async Task ApplyMigrationsAsync(WebApplication app)
{
    const int maxAttempts = 5;
    const int retrySeconds = 5;

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

            // Verificar que SQL Server está listo antes de migrar
            await db.Database.CanConnectAsync();

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

            // Liberar conexión fallida antes de reintentar
            await db.DisposeAsync();

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