using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Yarp.Gateway.Hubs;

namespace Yarp.Gateway.Consumers;

public class RoomEventsConsumer : 
    IConsumer<RoomReadyEvent>,
    IConsumer<CleaningStartedEvent>,
    IConsumer<DamageReportedEvent>,
    IConsumer<GuestCheckedInEvent>
{
    private readonly IHubContext<RoomHub> _hubContext;
    private readonly ILogger<RoomEventsConsumer> _logger;

    public RoomEventsConsumer(IHubContext<RoomHub> hubContext, ILogger<RoomEventsConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GuestCheckedInEvent> context)
    {
        _logger.LogInformation("SignalR: {RoomNumber} numaralı odaya {Guest} GİRİŞ YAPTI!", context.Message.RoomNumber, context.Message.GuestName);
        await _hubContext.Clients.All.SendAsync("ReceiveRoomStatus", new
        {
            eventType = "GuestCheckedIn",
            roomNumber = context.Message.RoomNumber,
            guestName = context.Message.GuestName,
            correlationId = context.Message.CorrelationId
        });
    }

    public async Task Consume(ConsumeContext<RoomReadyEvent> context)
    {
        _logger.LogInformation("SignalR: {RoomNumber} numaralı oda GİRİŞE HAZIR!", context.Message.RoomNumber);
        await _hubContext.Clients.All.SendAsync("ReceiveRoomStatus", new
        {
            eventType = "RoomReady",
            roomNumber = context.Message.RoomNumber,
            roomId = context.Message.RoomId,
            correlationId = context.Message.CorrelationId
        });
    }

    public async Task Consume(ConsumeContext<CleaningStartedEvent> context)
    {
        _logger.LogInformation("SignalR: {RoomNumber} numaralı odanın temizliği başladı. Temizlikçi: {Cleaner}", 
            context.Message.RoomNumber, context.Message.CleanerName);

        await _hubContext.Clients.All.SendAsync("ReceiveRoomStatus", new
        {
            eventType = "CleaningStarted",
            roomNumber = context.Message.RoomNumber,
            cleaner = context.Message.CleanerName,
            correlationId = context.Message.CorrelationId
        });
    }

    public async Task Consume(ConsumeContext<DamageReportedEvent> context)
    {
        _logger.LogWarning("SignalR: {RoomNumber} numaralı odada ARIZA bildirildi!", context.Message.RoomNumber);
        await _hubContext.Clients.All.SendAsync("ReceiveRoomStatus", new
        {
            eventType = "DamageReported",
            roomNumber = context.Message.RoomNumber,
            description = context.Message.Description,
            reportedBy = context.Message.ReportedBy,
            correlationId = context.Message.CorrelationId
        });
    }
}
