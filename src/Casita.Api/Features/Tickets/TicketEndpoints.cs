using Casita.Api.Features.Auth;
using Casita.Infrastructure.Homes;
using Casita.Infrastructure.Models;
using Casita.Infrastructure.Tickets;

namespace Casita.Api.Features.Tickets;

public static class TicketEndpoints
{
    public static void MapTicketEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/tickets").RequireAuthorization();

        group.MapPost("", async (
            CreateTicketRequest request,
            ITicketService ticketService,
            IHomeMembershipRepository memberships,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!Guid.TryParse(currentUser.UserId, out var userId))
            {
                return Results.Forbid();
            }

            // AuthZ: caller must belong to the target home.
            if (!await memberships.IsMemberAsync(request.HomeId, userId, ct))
            {
                return Results.Forbid();
            }

            var ticket = await ticketService.CreateTicketAsync(request, currentUser.UserId);
            return Results.Created($"/tickets/{ticket.Id}", ticket);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            ITicketRepository tickets,
            IHomeMembershipRepository memberships,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!Guid.TryParse(currentUser.UserId, out var userId))
            {
                return Results.NotFound();
            }

            var ticket = await tickets.GetByIdAsync(id, ct);
            if (ticket is null)
            {
                return Results.NotFound();
            }

            // Don't leak existence to non-members — return 404, not 403.
            if (!await memberships.IsMemberAsync(ticket.HomeId, userId, ct))
            {
                return Results.NotFound();
            }

            return Results.Ok(ticket);
        });
    }
}