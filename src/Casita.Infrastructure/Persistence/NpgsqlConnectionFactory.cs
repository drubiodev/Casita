using System.Data.Common;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Casita.Infrastructure.Persistence;

public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly PostgresOptions _options;

    public NpgsqlConnectionFactory(IOptions<PostgresOptions> options)
    {
        _options = options.Value;
    }

    public async Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
