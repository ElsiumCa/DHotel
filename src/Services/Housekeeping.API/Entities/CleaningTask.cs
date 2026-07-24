using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Housekeeping.API.Entities;

public enum CleaningStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3
}

public class CleaningTask
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;

    public CleaningStatus Status { get; set; } = CleaningStatus.Pending;
    public string? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
