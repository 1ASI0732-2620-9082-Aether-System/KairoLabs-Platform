using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class AddDevicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    exact_location = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    type_of_medication = table.Column<string>(type: "longtext", nullable: false),
                    temperature = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false),
                    humidity = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false),
                    light_intensity = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: false),
                    air_quality = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: false),
                    vibration = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    atmospheric_pressure = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    suspended_particles = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: false),
                    door_status = table.Column<string>(type: "longtext", nullable: false),
                    establishment_id = table.Column<int>(type: "int", nullable: false),
                    enabled_sensors = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, defaultValue: ""),
                    created_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_devices", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "devices");
        }
    }
}
