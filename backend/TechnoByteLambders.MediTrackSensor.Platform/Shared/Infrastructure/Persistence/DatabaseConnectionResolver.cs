namespace TechnoByteLambders.MediTrackSensor.Platform.Shared.Infrastructure.Persistence;

/// <summary>
/// Builds a MySQL connection string from Render / local environment variables.
/// </summary>
public static class DatabaseConnectionResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? Environment.GetEnvironmentVariable("MYSQL_URL")
            ?? Environment.GetEnvironmentVariable("MYSQL_PUBLIC_URL");

        if (!string.IsNullOrWhiteSpace(databaseUrl))
            return FromMySqlUrl(databaseUrl);

        var host = FirstNonEmpty(
            Environment.GetEnvironmentVariable("DATABASE_HOST"),
            Environment.GetEnvironmentVariable("MYSQL_HOST"),
            Environment.GetEnvironmentVariable("DB_HOST"));
        var port = FirstNonEmpty(
            Environment.GetEnvironmentVariable("DATABASE_PORT"),
            Environment.GetEnvironmentVariable("MYSQL_PORT"),
            Environment.GetEnvironmentVariable("DB_PORT"),
            "3306");
        var database = FirstNonEmpty(
            Environment.GetEnvironmentVariable("DATABASE_NAME"),
            Environment.GetEnvironmentVariable("MYSQL_DATABASE"),
            Environment.GetEnvironmentVariable("MYSQL_DB"),
            Environment.GetEnvironmentVariable("DB_NAME"));
        var user = FirstNonEmpty(
            Environment.GetEnvironmentVariable("DATABASE_USER"),
            Environment.GetEnvironmentVariable("MYSQL_USER"),
            Environment.GetEnvironmentVariable("DB_USER"));
        var password = FirstNonEmpty(
            Environment.GetEnvironmentVariable("DATABASE_PASSWORD"),
            Environment.GetEnvironmentVariable("MYSQL_PASSWORD"),
            Environment.GetEnvironmentVariable("DB_PASSWORD"));

        if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(database) &&
            !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
        {
            return WithMySqlOptions(
                $"server={host};port={port};database={database};user={user};password={password}");
        }

        var fromConfig = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(fromConfig))
        {
            fromConfig = Environment.ExpandEnvironmentVariables(fromConfig);
            if (!LooksLikeLocalPlaceholder(fromConfig))
                return WithMySqlOptions(fromConfig);
        }

        throw new InvalidOperationException(
            "Database connection is not configured. Set DATABASE_URL or DATABASE_HOST/PORT/NAME/USER/PASSWORD on Render.");
    }

    private static string FromMySqlUrl(string url)
    {
        if (!url.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
            return WithMySqlOptions(url);

        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port > 0 ? uri.Port : 3306;

        return WithMySqlOptions(
            $"server={uri.Host};port={port};database={database};user={user};password={password}");
    }

    private static string WithMySqlOptions(string connectionString)
    {
        var parts = connectionString.TrimEnd(';').Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
        var keys = new HashSet<string>(
            parts.Select(p => p.Split('=', 2)[0].Trim()),
            StringComparer.OrdinalIgnoreCase);

        void AddIfMissing(string segment)
        {
            var key = segment.Split('=', 2)[0].Trim();
            if (!keys.Contains(key))
                parts.Add(segment);
        }

        var server = parts
            .Select(p => p.Split('=', 2))
            .FirstOrDefault(p => p[0].Equals("server", StringComparison.OrdinalIgnoreCase)
                || p[0].Equals("host", StringComparison.OrdinalIgnoreCase))
            ?.ElementAtOrDefault(1);

        // Render private MySQL uses internal hostnames like "mysql-abc" (no dots) — no SSL.
        var sslMode = !string.IsNullOrEmpty(server) && !server.Contains('.')
            ? "SslMode=None"
            : "SslMode=Preferred";

        AddIfMissing(sslMode);
        AddIfMissing("CharSet=utf8mb4");
        AddIfMissing("AllowPublicKeyRetrieval=true");
        AddIfMissing("Connection Timeout=60");
        AddIfMissing("Default Command Timeout=60");

        return string.Join(';', parts) + ";";
    }

    private static bool LooksLikeLocalPlaceholder(string connectionString) =>
        connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("127.0.0.1") ||
        connectionString.Contains("${", StringComparison.Ordinal);

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;
}
