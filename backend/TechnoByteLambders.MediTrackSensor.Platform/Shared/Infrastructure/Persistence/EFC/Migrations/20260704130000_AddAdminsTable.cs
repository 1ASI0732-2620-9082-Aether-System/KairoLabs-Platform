using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    entity_name = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: false),
                    entity_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    schedule = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    users_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_admins", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "admins");
        }
    }
}
