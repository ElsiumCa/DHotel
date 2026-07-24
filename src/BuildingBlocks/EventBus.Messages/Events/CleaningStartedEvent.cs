using EventBus.Messages.Contracts;

namespace EventBus.Messages.Events;

public class CleaningStartedEvent : BaseMessage
{
    public string CleanerName { get; set; } = string.Empty;
}
