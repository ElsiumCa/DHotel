using EventBus.Messages.Contracts;

namespace EventBus.Messages.Commands;

public class CreateMaintenanceTicketCommand : BaseMessage
{
    public string Description { get; set; } = string.Empty;
}
