using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimitConsulta.Domain.Entities;

namespace SimitConsulta.Infrastructure.Persistence.Configurations;

/// <summary>Configuración Fluent API para SummonsDetail.</summary>
public class SummonsDetailConfiguration
    : IEntityTypeConfiguration<SummonsDetail>
{
    public void Configure(EntityTypeBuilder<SummonsDetail> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Number).HasMaxLength(50);
        b.Property(x => x.Status).HasMaxLength(50);
        b.Property(x => x.Infraction).HasMaxLength(500);
        b.Property(x => x.InfractionDate).HasMaxLength(20);
    }
}