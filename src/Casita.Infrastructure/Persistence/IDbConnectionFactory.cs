using System.Data.Common;

namespace Casita.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
