using Casita.Infrastructure.Models;
using Casita.Infrastructure.Tickets;

namespace Casita.Api.Features.Tickets;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;

    public TicketService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Ticket> CreateTicketAsync(CreateTicketRequest request)
    {
        // TODO: Add validation logic
        var now = DateTime.UtcNow;
        var ticket = new Ticket(
            Guid.CreateVersion7(),
            request.HomeId,
            request.assignedTo,
            request.Title,
            request.Description,
            request.Severity,
            request.DueDate,
            now,
            now
        );

        await _ticketRepository.InsertAsync(ticket);

        // TODO: publish events

        return ticket;
    }
}