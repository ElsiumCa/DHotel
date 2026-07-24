namespace EventBus.Messages.Contracts;

public interface IBaseMessage
{
    Guid CorrelationId { get; set; }
    DateTime CreationDate { get; set; }
    Guid RoomId { get; set; }
    string RoomNumber { get; set; }
}

public abstract class BaseMessage : IBaseMessage
{
    public Guid CorrelationId { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
}
