using Microsoft.EntityFrameworkCore;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.Aggregates;
using TechnoByteLambders.MediTrackSensor.Platform.Establishments.Domain.Model.ValueObjects;

namespace TechnoByteLambders.MediTrackSensor.Platform.Establishments.Infrastructure.Persistence.EFC.Configuration;

public static class EstablishmentsContextConfiguration
{
    public static void ApplyEstablishmentsConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Establishment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EstablishmentName).HasColumnName("establishment_name").IsRequired().HasMaxLength(150);
            entity.Property(e => e.EstablishmentType).HasColumnName("establishment_type").IsRequired().HasConversion<string>();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(15);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.Website).HasColumnName("website").HasMaxLength(150);

            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property<int>("EstablishmentId").HasColumnName("id");
                address.Property(a => a.Street).HasColumnName("address").HasMaxLength(200);
                address.Property(a => a.District).HasColumnName("district").HasMaxLength(100);
                address.Property(a => a.CityRegion).HasColumnName("city_region").HasMaxLength(100);
                address.Property(a => a.Country).HasColumnName("country").HasMaxLength(100);
            });

            entity.OwnsOne(e => e.Location, location =>
            {
                location.Property<int>("EstablishmentId").HasColumnName("id");
                location.Property(l => l.Latitude).HasColumnName("latitude").HasPrecision(10, 8);
                location.Property(l => l.Longitude).HasColumnName("longitude").HasPrecision(11, 8);
            });

            entity.Property(e => e.AdminId)
                .HasConversion(v => v.Value, v => new AdminId(v))
                .HasColumnName("admin_id");
        });

        builder.Entity<Operator>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasColumnName("id");
            entity.Property(o => o.AlertsAnswered).HasColumnName("alerts_answered").IsRequired();
            entity.Property(o => o.Schedule).HasColumnName("schedule").IsRequired().HasMaxLength(100);
            entity.Property(o => o.EstablishmentId).HasColumnName("establishment_id").IsRequired();

            entity.Property(o => o.UserId)
                .HasConversion(v => v.Value, v => new UserId(v))
                .HasColumnName("users_id");

            entity.HasOne<Establishment>()
                .WithMany()
                .HasForeignKey(o => o.EstablishmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
