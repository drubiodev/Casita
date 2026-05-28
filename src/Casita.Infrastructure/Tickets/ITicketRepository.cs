using Casita.Infrastructure.Models;

namespace Casita.Infrastructure.Tickets;

public interface ITicketRepository
{
    Task InsertAsync(Ticket ticket, CancellationToken cancellationToken = default);

    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
