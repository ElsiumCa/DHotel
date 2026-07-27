namespace Maintenance.API.Entities;

public enum TicketStatus
{
    Open = 1,
    InProgress = 2,
    Resolved = 3
}

public class MaintenanceTicket
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CorrelationId { get; set; }

    public Guid RoomId { get; set; }

    public string RoomNumber { get; set; } = String.Empty;

    public string Description { get; set; } = String.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public string? AssignedTechnician { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }
}