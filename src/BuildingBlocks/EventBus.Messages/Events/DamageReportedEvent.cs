using EventBus.Messages.Contracts;

namespace EventBus.Messages.Events;

public class DamageReportedEvent : BaseMessage
{
    public string Description { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
}
