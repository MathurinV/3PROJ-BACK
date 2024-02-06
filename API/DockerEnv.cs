namespace API;

public static class DockerEnv
{
    public static string PostgresUser { get; } = Environment.GetEnvironmentVariable("POSTGRES_USER") ??throw new Exception("POSTGRES_USER is not set");
    public static string PostgresPassword { get; } = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ??throw new Exception("POSTGRES_PASSWORD is not set");
    public static string PostgresDb { get; } = Environment.GetEnvironmentVariable("POSTGRES_DB") ??throw new Exception("POSTGRES_DB is not set");
    public static string DbPort { get; } = Environment.GetEnvironmentVariable("DB_PORT") ?? throw new Exception("DB_PORT is not set");
    public static string ApiEndpoint { get; } = Environment.GetEnvironmentVariable("API_ENDPOINT") ??throw new Exception("API_ENDPOINT is not set");
    public static string ConnectionString { get; } = $"Host=db;Port={DbPort};Database={PostgresDb};Username={PostgresUser};Password={PostgresPassword}";
}