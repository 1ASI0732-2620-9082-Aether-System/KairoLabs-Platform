using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    current_temperature = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false),
                    current_humidity = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false),
                    last_sensor_update = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_transports", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transports");
        }
    }
}
