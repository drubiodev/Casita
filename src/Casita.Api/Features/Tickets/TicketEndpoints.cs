using Casita.Infrastructure.Models;

namespace Casita.Api.Features.Tickets;

public static class TicketEndpoints
{
    public static void MapTicketEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/tickets");

        group.MapPost("", (CreateTicketRequest ticket, ITicketService ticketService) => ticketService.CreateTicketAsync(ticket));
    }
}