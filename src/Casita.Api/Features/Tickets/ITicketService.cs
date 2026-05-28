using Casita.Infrastructure.Models;

namespace Casita.Api.Features.Tickets;

public interface ITicketService
{
    Task<Ticket> CreateTicketAsync(CreateTicketRequest ticket);
}