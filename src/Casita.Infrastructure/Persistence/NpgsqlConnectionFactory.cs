using System.Data.Common;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Casita.Infrastructure.Persistence;

public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly PostgresOptions _options;
    private readonly ICurrentUserAccessor _currentUser;

    public NpgsqlConnectionFactory(
        IOptions<PostgresOptions> options,
        ICurrentUserAccessor currentUser)
    {
        _options = options.Value;
        _currentUser = currentUser;
    }

    public async Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.TryGetUserId();

        // Admin/init paths (no authenticated user) use the owner connection.
        // Request paths use the lower-privileged app role so RLS applies.
        var connectionString = userId is null
            ? _options.ConnectionString
            : _options.AppConnectionString ?? _options.ConnectionString;

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        if (userId is { } uid)
        {
            // Sets a session GUC read by RLS policies (current_user_id()).
            // Npgsql resets session state (DISCARD ALL) when the connection
            // returns to the pool, so the next caller starts clean.
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT set_config('app.user_id', $1, false)";
            var p = cmd.CreateParameter();
            p.Value = uid.ToString();
            cmd.Parameters.Add(p);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        return connection;
    }
}

