using EventBus.Messages.Commands;
using Housekeeping.API.Entities;
using MassTransit;
using MongoDB.Driver;

namespace Housekeeping.API.Consumers;

public class AssignCleaningTaskConsumer : IConsumer<AssignCleaningTaskCommand>
{
    private readonly IMongoCollection<CleaningTask> _taskCollection;
    private readonly ILogger<AssignCleaningTaskConsumer> _logger;

    public AssignCleaningTaskConsumer(IMongoDatabase database, ILogger<AssignCleaningTaskConsumer> logger)
    {
        _taskCollection = database.GetCollection<CleaningTask>("CleaningTasks");
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AssignCleaningTaskCommand> context)
    {
        var command = context.Message;
        _logger.LogInformation("Temizlik görevi alındı. RoomId: {RoomId}, CorrelationId: {CorrelationId}", command.RoomId, command.CorrelationId);

        var task = new CleaningTask
        {
            CorrelationId = command.CorrelationId,
            RoomId = command.RoomId,
            RoomNumber = command.RoomNumber,
            Status = CleaningStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _taskCollection.InsertOneAsync(task);
    }
}
