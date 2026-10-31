using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class DatabaseMigrationExtensions
{
    private static readonly (string MigrationId, string TableName, string ProductVersion)[] KnownMigrations =
    [
        ("20260609120000_AddUsersTable", "users", "9.0.6"),
        ("20260610130000_AddDevicesTable", "devices", "9.0.6"),
        ("20260702093209_AddEstablishmentsOperatorsSubscriptions", "establishments", "9.0.6"),
        ("20260703042413_AddTransportsTable", "transports", "10.0.1"),
        ("20260704130000_AddAdminsTable", "admins", "10.0.1"),
    ];

    public static void ApplyPendingMigrations(this DatabaseFacade database)
    {
        SyncMigrationHistoryIfNeeded(database);
        EnsureAdminsTable(database);
        EnsureTransportsTable(database);

        // If core tables are missing but history says migrated, reset and re-apply.
        if (!TableExists(database, "users"))
            database.ExecuteSqlRaw("DELETE FROM `__EFMigrationsHistory`");

        database.GetService<IMigrator>().Migrate();
    }

    private static void EnsureAdminsTable(DatabaseFacade database)
    {
        if (TableHasColumn(database, "admins", "entity_name"))
            return;

        if (TableExists(database, "admins"))
            database.ExecuteSqlRaw("DROP TABLE `admins`");

        const string migrationId = "20260704130000_AddAdminsTable";

        database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS `admins` (
                `id` int NOT NULL AUTO_INCREMENT,
                `entity_name` varchar(400) NOT NULL,
                `entity_code` varchar(20) NOT NULL,
                `schedule` varchar(100) NOT NULL,
                `users_id` int NOT NULL,
                `created_at` datetime NULL,
                `updated_at` datetime NULL,
                PRIMARY KEY (`id`)
            ) CHARACTER SET utf8mb4;
            """);

        database.ExecuteSqlRaw(
            "DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = {0}",
            migrationId);
        database.ExecuteSqlRaw(
            "INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES ({0}, {1})",
            migrationId,
            "10.0.1");
    }

    private static void EnsureTransportsTable(DatabaseFacade database)
    {
        if (TableHasColumn(database, "transports", "type_of_transport"))
            return;

        if (TableExists(database, "transports"))
            database.ExecuteSqlRaw("DROP TABLE `transports`");

        const string migrationId = "20260703042413_AddTransportsTable";

        database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS `transports` (
                `id` int NOT NULL AUTO_INCREMENT,
                `type_of_transport` varchar(100) NOT NULL,
                `type_of_medication` varchar(50) NOT NULL,
                `door_status` varchar(50) NOT NULL DEFAULT 'Closed',
                `temperature` decimal(4,1) NOT NULL DEFAULT 0,
                `humidity` decimal(4,1) NOT NULL DEFAULT 0,
                `light_intensity` decimal(6,1) NOT NULL DEFAULT 0,
                `air_quality` decimal(6,1) NOT NULL DEFAULT 0,
                `vibration` decimal(5,2) NOT NULL DEFAULT 0,
                `atmospheric_pressure` decimal(6,2) NOT NULL DEFAULT 0,
                `suspended_particles` decimal(5,1) NOT NULL DEFAULT 0,
                `establishment_id` int NOT NULL,
                `enabled_sensors` varchar(500) NOT NULL DEFAULT '',
                `created_at` datetime NULL,
                `updated_at` datetime NULL,
                PRIMARY KEY (`id`)
            ) CHARACTER SET utf8mb4;
            """);

        database.ExecuteSqlRaw(
            "DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = {0}",
            migrationId);
        database.ExecuteSqlRaw(
            "INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES ({0}, {1})",
            migrationId,
            "10.0.1");
    }

    private static void SyncMigrationHistoryIfNeeded(DatabaseFacade database)
    {
        database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
                `MigrationId` varchar(150) NOT NULL,
                `ProductVersion` varchar(32) NOT NULL,
                PRIMARY KEY (`MigrationId`)
            );
            """);

        foreach (var (migrationId, tableName, productVersion) in KnownMigrations)
        {
            var tableExists = TableExists(database, tableName);
            var migrationExists = MigrationExists(database, migrationId);

            if (migrationExists && !tableExists)
            {
                database.ExecuteSqlRaw(
                    "DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = {0}",
                    migrationId);
                continue;
            }

            if (!tableExists || migrationExists)
                continue;

            database.ExecuteSqlRaw(
                "INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES ({0}, {1})",
                migrationId,
                productVersion);
        }
    }

    private static bool TableHasColumn(DatabaseFacade database, string tableName, string columnName)
    {
        if (!TableExists(database, tableName))
            return false;

        var connection = database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            connection.Open();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = @tableName AND column_name = @columnName";
            var tableParam = command.CreateParameter();
            tableParam.ParameterName = "@tableName";
            tableParam.Value = tableName;
            command.Parameters.Add(tableParam);
            var columnParam = command.CreateParameter();
            columnParam.ParameterName = "@columnName";
            columnParam.Value = columnName;
            command.Parameters.Add(columnParam);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
        finally
        {
            if (shouldClose)
                connection.Close();
        }
    }

    private static bool TableExists(DatabaseFacade database, string tableName)
    {
        var connection = database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            connection.Open();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @tableName";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
        finally
        {
            if (shouldClose)
                connection.Close();
        }
    }

    private static bool MigrationExists(DatabaseFacade database, string migrationId)
    {
        var connection = database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            connection.Open();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT COUNT(*) FROM `__EFMigrationsHistory` WHERE `MigrationId` = @migrationId";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@migrationId";
            parameter.Value = migrationId;
            command.Parameters.Add(parameter);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
        finally
        {
            if (shouldClose)
                connection.Close();
        }
    }
}
