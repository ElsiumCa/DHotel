using EventBus.Messages.Contracts;

namespace EventBus.Messages.Events;

public class GuestCheckedInEvent : BaseMessage
{
    public string GuestName { get; set; } = string.Empty;
}
