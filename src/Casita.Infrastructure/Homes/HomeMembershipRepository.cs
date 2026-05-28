using Casita.Infrastructure.Persistence;
using Dapper;

namespace Casita.Infrastructure.Homes;

public class HomeMembershipRepository : IHomeMembershipRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public HomeMembershipRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> IsMemberAsync(Guid homeId, Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM home_members
                WHERE home_id = @HomeId AND user_id = @UserId
            );
            """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            sql,
            new { HomeId = homeId, UserId = userId },
            cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(command);
    }
}
