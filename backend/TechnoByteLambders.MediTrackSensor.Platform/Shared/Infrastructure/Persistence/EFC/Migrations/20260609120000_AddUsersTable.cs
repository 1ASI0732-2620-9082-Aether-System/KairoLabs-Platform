using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    dni = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false),
                    job_title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    role = table.Column<string>(type: "longtext", nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    photo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_users", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "users");
        }
    }
}