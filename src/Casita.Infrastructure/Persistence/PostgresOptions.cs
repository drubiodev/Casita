namespace Casita.Infrastructure.Persistence;

public class PostgresOptions
{
    public const string SectionName = "Postgres";

    public required string ConnectionString { get; set; }
}
