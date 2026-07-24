using EventBus.Messages.Contracts;

namespace EventBus.Messages.Events;

public class GuestCheckedOutEvent : BaseMessage
{
    public Guid GuestId { get; set; }
}
