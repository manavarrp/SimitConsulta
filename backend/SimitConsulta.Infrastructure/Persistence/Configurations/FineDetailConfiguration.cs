using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimitConsulta.Domain.Entities;

namespace SimitConsulta.Infrastructure.Persistence.Configurations;

/// <summary>Configuración Fluent API para FineDetail.</summary>
public class FineDetailConfiguration
    : IEntityTypeConfiguration<FineDetail>
{
    public void Configure(EntityTypeBuilder<FineDetail> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Number).HasMaxLength(50);
        b.Property(x => x.Status).HasMaxLength(50);
        b.Property(x => x.Agency).HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.InfractionDate).HasMaxLength(20);
    }
}