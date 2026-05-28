namespace Casita.Infrastructure.Models;

public record Ticket(
    Guid Id,
    Guid HomeId,
    Guid? AssignedTo,
    string Title,
    string Description,
    Severity Severity,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
