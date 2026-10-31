using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Monitoring.Infrastructure.Persistence.EFC.Configuration;

public static class MonitoringContextConfiguration
{
    public static void ApplyMonitoringConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Device>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).HasColumnName("id");
            entity.Property(d => d.ExactLocation).HasColumnName("exact_location").HasMaxLength(150);
            entity.Property(d => d.TypeOfMedication).HasColumnName("type_of_medication").IsRequired().HasConversion<string>();
            entity.Property(d => d.DoorStatus).HasColumnName("door_status").IsRequired().HasConversion<string>();

            // SensorReading: multi-property VO → OwnsOne (fix shadow key)
            entity.OwnsOne(d => d.SensorReading, sr =>
            {
                sr.Property<int>("DeviceId").HasColumnName("id");
                sr.Property(s => s.Temperature).HasColumnName("temperature").HasPrecision(4, 1);
                sr.Property(s => s.Humidity).HasColumnName("humidity").HasPrecision(4, 1);
                sr.Property(s => s.LightIntensity).HasColumnName("light_intensity").HasPrecision(6, 1);
                sr.Property(s => s.AirQuality).HasColumnName("air_quality").HasPrecision(6, 1);
                sr.Property(s => s.Vibration).HasColumnName("vibration").HasPrecision(5, 2);
                sr.Property(s => s.AtmosphericPressure).HasColumnName("atmospheric_pressure").HasPrecision(6, 2);
                sr.Property(s => s.SuspendedParticles).HasColumnName("suspended_particles").HasPrecision(5, 1);
            });

            // EstablishmentId: single-property VO → HasConversion
            entity.Property(d => d.EstablishmentId)
                .HasConversion(v => v.Value, v => new EstablishmentId(v))
                .HasColumnName("establishment_id");

            entity.Property(d => d.EnabledSensors)
                .HasColumnName("enabled_sensors")
                .HasMaxLength(500)
                .HasDefaultValue("");
        });
    }
}
