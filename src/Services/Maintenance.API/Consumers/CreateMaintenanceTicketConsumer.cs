using EventBus.Messages.Commands;
using Maintenance.API.Data;
using Maintenance.API.Entities;
using MassTransit;

namespace Maintenance.API.Consumers;

public class CreateMaintenanceTicketConsumer : IConsumer<CreateMaintenanceTicketCommand>
{
    private readonly MaintenanceDbContext _context;
    private readonly ILogger<CreateMaintenanceTicketConsumer> _logger;

    public CreateMaintenanceTicketConsumer(
        MaintenanceDbContext context,
        ILogger<CreateMaintenanceTicketConsumer> logger
    )
    {
        _context = context;
        _logger = logger;

    }

    public async Task Consume(ConsumeContext<CreateMaintenanceTicketCommand> context)
    {
        var command = context.Message;

           _logger.LogInformation("Arıza bileti oluşturma komutu alındı. RoomNumber: {RoomNumber}, Description: {Description}", 
            command.RoomNumber, command.Description);

        var ticket = new MaintenanceTicket { 
            CorrelationId = command.CorrelationId,
            RoomId = command.RoomId,
            RoomNumber = command.RoomNumber,
            Description = command.Description,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        await _context.maintenanceTickets.AddAsync(ticket);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Arıza kaydı veritabanına oluşturuldu. TicketId: {TicketId}", ticket.Id);
    }
}