namespace Casita.Api.Models;

public record Ticket(
    Guid Id,
    Guid HomeId,
    Guid AssignedTo,
    string Title,
    string Description,
    int Severity,
    DateTime DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);