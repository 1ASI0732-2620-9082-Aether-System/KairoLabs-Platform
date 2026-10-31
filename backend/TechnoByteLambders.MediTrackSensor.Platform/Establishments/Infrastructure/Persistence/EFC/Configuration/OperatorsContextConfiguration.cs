using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Infrastructure.Persistence.EFC.Configuration;

public static class OperatorsContextConfiguration
{
    public static void ApplyOperatorsConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Operator>(entity =>
        {
            entity.ToTable("operators");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AlertsAnswered).HasColumnName("alerts_answered").IsRequired();
            entity.Property(e => e.Schedule).HasColumnName("schedule").IsRequired().HasMaxLength(100);
            entity.Property(e => e.EstablishmentId).HasColumnName("establishment_id").IsRequired();
            entity.Property(e => e.UserId)
                .HasConversion(v => v.Value, v => new UserId(v))
                .HasColumnName("user_id");
        });
    }
}
