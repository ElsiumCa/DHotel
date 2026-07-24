using EventBus.Messages.Events;
using Housekeeping.API.Entities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Housekeeping.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HousekeepingController : ControllerBase
{
    private readonly IMongoCollection<CleaningTask> _taskCollection;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<HousekeepingController> _logger;

    public HousekeepingController(
        IMongoDatabase database,
        IPublishEndpoint publishEndpoint,
        ILogger<HousekeepingController> logger)
    {
        _taskCollection = database.GetCollection<CleaningTask>("CleaningTasks");
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    // 1. Tüm Temizlik Görevlerini Getir
    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks()
    {
        var tasks = await _taskCollection.Find(_ => true).ToListAsync();
        return Ok(tasks);
    }

    // 2. Temizliğe Başla
    [HttpPost("tasks/{id}/start")]
    public async Task<IActionResult> StartCleaning(string id, [FromBody] string cleanerName)
    {
        var task = await _taskCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (task == null)
        {
            return NotFound("Temizlik görevi bulunamadı.");
        }

        task.Status = CleaningStatus.InProgress;
        task.StartedAt = DateTime.UtcNow;
        task.AssignedTo = cleanerName;

        await _taskCollection.ReplaceOneAsync(t => t.Id == id, task);

        // RabbitMQ'ya CleaningStartedEvent fırlat (Saga State Machine dinleyecek)
        await _publishEndpoint.Publish(new CleaningStartedEvent
        {
            CorrelationId = task.CorrelationId,
            RoomId = task.RoomId,
            RoomNumber = task.RoomNumber,
            CleanerName = cleanerName
        });

        _logger.LogInformation("Temizlik başladı. RoomNumber: {RoomNumber}", task.RoomNumber);
        return Ok(new { message = "Temizlik başladı bildirimi gönderildi.", task });
    }

    // 3. Temizliği Tamamla
    [HttpPost("tasks/{id}/finish")]
    public async Task<IActionResult> FinishCleaning(string id)
    {
        var task = await _taskCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (task == null)
        {
            return NotFound("Temizlik görevi bulunamadı.");
        }

        task.Status = CleaningStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;

        await _taskCollection.ReplaceOneAsync(t => t.Id == id, task);

        // RabbitMQ'ya CleaningFinishedEvent fırlat (Saga oda durumunu 'ReadyForCheckIn' yapacak)
        await _publishEndpoint.Publish(new CleaningFinishedEvent
        {
            CorrelationId = task.CorrelationId,
            RoomId = task.RoomId,
            RoomNumber = task.RoomNumber
        });

        _logger.LogInformation("Temizlik tamamlandı. RoomNumber: {RoomNumber}", task.RoomNumber);
        return Ok(new { message = "Temizlik tamamlandı bildirimi gönderildi.", task });
    }

    // 4. Arıza/Hasar Bildir
    [HttpPost("tasks/{id}/report-damage")]
    public async Task<IActionResult> ReportDamage(string id, [FromBody] string description)
    {
        var task = await _taskCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (task == null)
        {
            return NotFound("Temizlik görevi bulunamadı.");
        }

        // RabbitMQ'ya DamageReportedEvent fırlat (Saga odanın durumunu 'InMaintenance' yapacak)
        await _publishEndpoint.Publish(new DamageReportedEvent
        {
            CorrelationId = task.CorrelationId,
            RoomId = task.RoomId,
            RoomNumber = task.RoomNumber,
            Description = description,
            ReportedBy = task.AssignedTo ?? "Housekeeper"
        });

        _logger.LogWarning("Oda arızası bildirildi. RoomNumber: {RoomNumber}, Açıklama: {Description}", task.RoomNumber, description);
        return Ok(new { message = "Arıza bildirimi gönderildi." });
    }
}
