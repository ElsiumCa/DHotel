using EventBus.Messages.Events;
using FrontDesk.API.Data;
using FrontDesk.API.Entities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrontDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly FrontDeskDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(FrontDeskDbContext context, IPublishEndpoint publishEndpoint, ILogger<CheckoutController> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost("{reservationId}")]
    public async Task<IActionResult> Checkout(string reservationId, [FromBody] CheckoutRequest? request)
    {
        var roomNumber = request?.RoomNumber ?? "101";

        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
        if (room == null)
        {
            room = new Room { Id = Guid.NewGuid(), RoomNumber = roomNumber, RoomType = "Standard", Price = 100 };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
        }

        var correlationId = Guid.NewGuid();

        await _publishEndpoint.Publish(new GuestCheckedOutEvent
        {
            CorrelationId = correlationId,
            RoomId = room.Id,
            RoomNumber = room.RoomNumber,
            GuestId = Guid.NewGuid(),
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Müşteri çıkış yaptı. RoomNumber: {RoomNumber}, CorrelationId: {CorrelationId}", 
            room.RoomNumber, correlationId);

        return Ok(new
        {
            message = "Müşteri çıkış işlemi başarılı. Temizlik süreci başlatıldı.",
            correlationId = correlationId,
            roomId = room.Id,
            roomNumber = room.RoomNumber
        });
    }
}

public class CheckoutRequest
{
    public string? RoomNumber { get; set; }
}
        
       



