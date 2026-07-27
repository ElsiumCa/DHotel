using EventBus.Messages.Events;
using FrontDesk.API.Data;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace FrontDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckinController : ControllerBase
{
    private readonly FrontDeskDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CheckinController> _logger;

    public CheckinController(FrontDeskDbContext context, IPublishEndpoint publishEndpoint, ILogger<CheckinController> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Checkin([FromBody] CheckinRequest request)
    {
        var correlationId = request.CorrelationId != Guid.Empty ? request.CorrelationId : Guid.NewGuid();

        await _publishEndpoint.Publish(new GuestCheckedInEvent
        {
            CorrelationId = correlationId,
            RoomId = request.RoomId,
            RoomNumber = request.RoomNumber,
            GuestName = request.GuestName
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Misafir girişi yapıldı. RoomNumber: {RoomNumber}, Guest: {GuestName}", 
            request.RoomNumber, request.GuestName);

        return Ok(new
        {
            message = "Misafir girişi başarılı. Oda dolu durumuna geçti.",
            correlationId = correlationId,
            roomNumber = request.RoomNumber
        });
    }
}

public class CheckinRequest
{
    public Guid CorrelationId { get; set; }
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
}
