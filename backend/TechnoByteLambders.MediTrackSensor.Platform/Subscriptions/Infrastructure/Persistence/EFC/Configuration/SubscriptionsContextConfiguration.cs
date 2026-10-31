using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Subscriptions.Infrastructure.Persistence.EFC.Configuration;

public static class SubscriptionsContextConfiguration
{
    public static void ApplySubscriptionsConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Subscription>(entity =>
        {
            entity.ToTable("subscriptions");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasColumnName("id");
            entity.Property(s => s.Plan).HasColumnName("plan").IsRequired().HasConversion<string>();
            entity.Property(s => s.Status).HasColumnName("status").IsRequired().HasConversion<string>();
            entity.Property(s => s.StartDate)
                .HasColumnName("start_date")
                .HasConversion(v => v.ToDateTime(TimeOnly.MinValue), v => DateOnly.FromDateTime(v))
                .IsRequired();
            entity.Property(s => s.EndDate)
                .HasColumnName("end_date")
                .HasConversion(v => v.ToDateTime(TimeOnly.MinValue), v => DateOnly.FromDateTime(v))
                .IsRequired();
            entity.Property(s => s.AdminId)
                .HasConversion(v => v.Value, v => new AdminId(v))
                .HasColumnName("admin_id");
            entity.Property(s => s.CreatedAt).HasColumnName("created_at");
        });
    }
}
