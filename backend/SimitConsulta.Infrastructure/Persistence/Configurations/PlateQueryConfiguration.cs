using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimitConsulta.Domain.Entities;

namespace SimitConsulta.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API de EF Core para PlateQuery.
/// Define tipos de columna, índices y relaciones.
/// </summary>
public class PlateQueryConfiguration
    : IEntityTypeConfiguration<PlateQuery>
{
    public void Configure(EntityTypeBuilder<PlateQuery> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.Plate)
            .HasMaxLength(10)
            .IsRequired();

        // Enums persistidos como string para que la BD sea legible
        b.Property(x => x.QueryType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        b.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        b.Property(x => x.RawResponse)
            .HasColumnType("nvarchar(max)");

        b.Property(x => x.ErrorMessage)
            .HasMaxLength(500);

        b.Property(x => x.TotalAmount)
            .HasColumnType("decimal(18,2)");

        // Índice sobre Plate — acelera filtros del historial
        b.HasIndex(x => x.Plate)
            .HasDatabaseName("IX_PlateQueries_Plate");

        // Índice descendente sobre fecha — historial más reciente primero
        b.HasIndex(x => x.ConsultedAt)
            .HasDatabaseName("IX_PlateQueries_ConsultedAt");

        // Relaciones con cascade delete
        b.HasMany(x => x.Fines)
            .WithOne()
            .HasForeignKey("PlateQueryId")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Summons)
            .WithOne()
            .HasForeignKey("PlateQueryId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}