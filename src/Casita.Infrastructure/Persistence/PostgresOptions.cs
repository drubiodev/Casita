namespace Casita.Infrastructure.Persistence;

public class PostgresOptions
{
    public const string SectionName = "Postgres";

    /// <summary>
    /// Admin/migration connection (typically the database owner / superuser).
    /// Used by <see cref="DatabaseInitializer"/> and any caller without an
    /// authenticated user context.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Optional non-superuser connection used for request-scoped queries so
    /// row-level security applies. Falls back to <see cref="ConnectionString"/>
    /// if not configured.
    /// </summary>
    public string? AppConnectionString { get; set; }
}

