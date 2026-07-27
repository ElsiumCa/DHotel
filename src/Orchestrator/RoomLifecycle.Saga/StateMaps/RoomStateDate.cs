

using MassTransit;

namespace RoomLifecycle.Saga.StateMaps;

public class RoomStateData : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }

    public string CurrentState { get; set; } = String.Empty;

    public Guid Roomid { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}