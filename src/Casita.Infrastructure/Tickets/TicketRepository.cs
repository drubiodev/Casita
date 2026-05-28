using Casita.Infrastructure.Models;
using Casita.Infrastructure.Persistence;
using Dapper;

namespace Casita.Infrastructure.Tickets;

public class TicketRepository : ITicketRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TicketRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO tickets (
                id, home_id, assigned_to, title, description,
                severity, due_date, created_at, updated_at, created_by
            )
            VALUES (
                @Id, @HomeId, @AssignedTo, @Title, @Description,
                @Severity, @DueDate, @CreatedAt, @UpdatedAt, @CreatedBy
            );
            """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, ticket, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                id          AS Id,
                home_id     AS HomeId,
                assigned_to AS AssignedTo,
                title       AS Title,
                description AS Description,
                severity    AS Severity,
                due_date    AS DueDate,
                created_at  AS CreatedAt,
                updated_at  AS UpdatedAt,
                created_by  AS CreatedBy
            FROM tickets
            WHERE id = @Id;
            """;

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Ticket>(command);
    }
}
