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
    public async Task<IActionResult> Checkout(Guid reservationId)
    {
        var reservation = await _context.Reservations
        .Include(r => r.Room)
        .Include(r => r.Guest)
        .FirstOrDefaultAsync(r => r.Id == reservationId);

        if(reservation == null)
        {
            return NotFound("Rezervasyon bulunamadı.");
        }

        if (reservation.Status == ReservationStatus.CheckedOut)
        {
            return BadRequest("Rezervasyon zaten tamamlandı.");
        }

        var CorrelationId = Guid.NewGuid();

        reservation.Status = ReservationStatus.CheckedOut;

        await _publishEndpoint.Publish(new GuestCheckedOutEvent
        {
            CorrelationId = CorrelationId,
            RoomId = reservation.RoomId,
            RoomNumber = reservation.Room!.RoomNumber,
            GuestId = reservation.GuestId,
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Müşteri çıkış yaptı. RoomNumber: {RoomNumber}, CorrelationId: {CorrelationId}", 
            reservation.Room?.RoomNumber, CorrelationId);
        return Ok(new
        {
            message = "Müşteri çıkış işlemi başarılı. Temizlik süreci başlatıldı.",
            correlationId = CorrelationId,
            roomId = reservation.RoomId,
            roomNumber = reservation.Room?.RoomNumber
        });


    }
        
       
    }


