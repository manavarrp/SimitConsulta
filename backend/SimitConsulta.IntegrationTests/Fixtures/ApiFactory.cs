using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimitConsulta.Application.Interfaces;
using SimitConsulta.Infrastructure.Persistence;

namespace SimitConsulta.IntegrationTests.Fixtures;

/// <summary>
/// Fixture para integration tests.
/// Reemplaza SQL Server con InMemory y ISimitGateway con mock.
/// El entorno "Testing" hace que Program.cs salte las migraciones
/// — InMemory no soporta MigrateAsync(), usa EnsureCreated().
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public Mock<ISimitGateway> SimitGatewayMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Reemplazar SQL Server con InMemory
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<AppDbContext>));
            if (dbDescriptor is not null)
                services.Remove(dbDescriptor);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase(
                    $"TestDb_{Guid.NewGuid()}"));

            // Reemplazar ISimitGateway con mock
            var gatewayDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ISimitGateway));
            if (gatewayDescriptor is not null)
                services.Remove(gatewayDescriptor);

            services.AddScoped<ISimitGateway>(
                _ => SimitGatewayMock.Object);
        });

        // "Testing" hace que Program.cs salte ApplyMigrationsAsync
        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Crear la BD InMemory con EnsureCreated — no MigrateAsync.
    /// EnsureCreated crea el esquema directamente sin historial de migraciones.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Forzar que el servidor arranque antes de intentar acceder a Services
        _ = this.CreateClient();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider
                      .GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider
                      .GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }
}