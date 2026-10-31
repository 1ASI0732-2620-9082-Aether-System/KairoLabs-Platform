using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Logistics.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Logistics.Infrastructure.Persistence.EFC.Configuration;

public static class LogisticsContextConfiguration
{
    public static void ApplyLogisticsConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Transport>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.TypeOfTransport).HasColumnName("type_of_transport").IsRequired().HasMaxLength(100);
            entity.Property(t => t.TypeOfMedication).HasColumnName("type_of_medication").IsRequired().HasConversion<string>();
            entity.Property(t => t.DoorStatus).HasColumnName("door_status").IsRequired().HasConversion<string>();

            // SensorReading: multi-property VO → OwnsOne (fix shadow key)
            entity.OwnsOne(t => t.SensorReading, sr =>
            {
                sr.Property<int>("TransportId").HasColumnName("id");
                sr.Property(s => s.Temperature).HasColumnName("temperature").HasPrecision(4, 1);
                sr.Property(s => s.Humidity).HasColumnName("humidity").HasPrecision(4, 1);
                sr.Property(s => s.LightIntensity).HasColumnName("light_intensity").HasPrecision(6, 1);
                sr.Property(s => s.AirQuality).HasColumnName("air_quality").HasPrecision(6, 1);
                sr.Property(s => s.Vibration).HasColumnName("vibration").HasPrecision(5, 2);
                sr.Property(s => s.AtmosphericPressure).HasColumnName("atmospheric_pressure").HasPrecision(6, 2);
                sr.Property(s => s.SuspendedParticles).HasColumnName("suspended_particles").HasPrecision(5, 1);
            });

            // EstablishmentId: single-property VO → HasConversion
            entity.Property(t => t.EstablishmentId)
                .HasConversion(v => v.Value, v => new EstablishmentId(v))
                .HasColumnName("establishment_id");

            entity.Property(t => t.EnabledSensors)
                .HasColumnName("enabled_sensors")
                .HasMaxLength(500)
                .HasDefaultValue("");
        });
    }
}
