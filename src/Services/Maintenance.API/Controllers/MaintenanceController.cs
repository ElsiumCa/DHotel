
using Maintenance.API.Entities;
using Maintenance.API.Data;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.API.Controllers
{
    public class MaintenanceController : ControllerBase
    {
        private readonly MaintenanceDbContext _context;
        private readonly IPublishEndpoint _publishendpoint;
        private readonly ILogger _logger;

        public MaintenanceController(MaintenanceDbContext context,IPublishEndpoint publishEndpoint,ILogger logger)
        {
            _context = context;
            _publishendpoint = publishEndpoint;
            _logger = logger;
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets()
        {
            var tickets = await _context.maintenanceTickets.OrderByDescending(d => d.CreatedAt).ToListAsync();

            return Ok(tickets);
        }

        [HttpPost("tickets/{id}/resolve")]
        public async Task<IActionResult> ResolveTicket(Guid id,[FromBody] string technicianName)
        {
            var ticket = await _context.maintenanceTickets.FindAsync(id);
            if (ticket == null) { return NotFound("Aradığınız ticket bulunamadı"); }
            if (ticket.Status == TicketStatus.Resolved) { return BadRequest("Bu ticket zaten çozülmüs"); }

            ticket.Status = TicketStatus.Resolved;
            ticket.ResolvedAt = DateTime.UtcNow;
            ticket.AssignedTechnician = technicianName;

           await _publishendpoint.Publish(new EventBus.Messages.Events.MaintenanceResolvedEvent
            {
                CorrelationId = ticket.CorrelationId,
                RoomId = ticket.RoomId,
                RoomNumber = ticket.RoomNumber
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation("Arıza giderildi.Roomnumber:{RoomNumber},Teknisyen:{Tech}",
             ticket.RoomNumber, ticket.AssignedTechnician);

            return Ok(new
            {
                message = "Arıza giderildi bildirimi gonderildi.",
                ticket
            });

        }
    }
}