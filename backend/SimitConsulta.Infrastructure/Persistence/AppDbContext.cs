using Microsoft.EntityFrameworkCore;
using SimitConsulta.Domain.Entities;

namespace SimitConsulta.Infrastructure.Persistence;

/// <summary>
/// DbContext de EF Core para la aplicación.
/// ApplyConfigurationsFromAssembly auto-descubre todas las
/// configuraciones Fluent de la carpeta Configurations/.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    /// <summary>Tabla principal — aggregate root.</summary>
    public DbSet<PlateQuery> PlateQueries => Set<PlateQuery>();

    /// <summary>Tabla de multas — entidad hija.</summary>
    public DbSet<FineDetail> FineDetails => Set<FineDetail>();

    /// <summary>Tabla de comparendos — entidad hija.</summary>
    public DbSet<SummonsDetail> SummonsDetails => Set<SummonsDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Auto-descubre IEntityTypeConfiguration<T> del ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}