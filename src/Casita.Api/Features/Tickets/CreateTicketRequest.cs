using Casita.Infrastructure.Models;

namespace Casita.Api.Features.Tickets;

public class CreateTicketRequest
{
    public required Guid HomeId { get; set; }
    public Guid? assignedTo { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required Severity Severity { get; set; } = Severity.Low;
    public DateTime? DueDate { get; set; }
}