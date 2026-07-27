using EventBus.Messages.Commands;
using EventBus.Messages.Events;
using MassTransit;
using RoomLifecycle.Saga.StateMaps;

namespace RoomLifecycle.Saga.StateMachine;

public class RoomLifecycleStateMachine : MassTransitStateMachine<RoomStateData>
{
    public State AwaitingCleaning { get; private set; } = null!;
    public State InCleaning { get; private set; } = null!;
    public State InMaintenance { get; private set; } = null!;
    public State ReadyForCheckIn { get; private set; } = null!;
    public State Occupied { get; private set; } = null!;

    public Event<GuestCheckedOutEvent> GuestCheckedOut { get; private set; } = null!;
    public Event<CleaningStartedEvent> CleaningStarted { get; private set; } = null!;
    public Event<CleaningFinishedEvent> CleaningFinished { get; private set; } = null!;
    public Event<MaintenanceResolvedEvent> MaintenanceResolved { get; private set; } = null!;
    public Event<DamageReportedEvent> DamageReported { get; private set; } = null!;
    public Event<GuestCheckedInEvent> GuestCheckedIn { get; private set; } = null!;

    public RoomLifecycleStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => GuestCheckedOut, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CleaningStarted, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CleaningFinished, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => DamageReported, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => MaintenanceResolved, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => GuestCheckedIn, x => x.CorrelateById(m => m.Message.CorrelationId));

        Initially(
            When(GuestCheckedOut).Then(context =>
            {
                context.Saga.Roomid = context.Message.RoomId;
                context.Saga.RoomNumber = context.Message.RoomNumber;
                context.Saga.UpdatedAt = DateTime.UtcNow;
            })
            .TransitionTo(AwaitingCleaning)
            .Send(new Uri("queue:assign-cleaning-task-queue"), context => new AssignCleaningTaskCommand
            {
                CorrelationId = context.Saga.CorrelationId,
                RoomId = context.Saga.Roomid,
                RoomNumber = context.Saga.RoomNumber
            }),
            When(DamageReported).Then(context =>
            {
                context.Saga.Roomid = context.Message.RoomId;
                context.Saga.RoomNumber = context.Message.RoomNumber;
                context.Saga.UpdatedAt = DateTime.UtcNow;
            })
            .TransitionTo(InMaintenance)
            .Send(new Uri("queue:create-maintenance-ticket-queue"), context => new CreateMaintenanceTicketCommand
            {
                CorrelationId = context.Saga.CorrelationId,
                RoomId = context.Saga.Roomid,
                RoomNumber = context.Saga.RoomNumber,
                Description = context.Message.Description
            })
        );

        During(AwaitingCleaning,
            When(CleaningStarted)
            .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
            .TransitionTo(InCleaning)
        );

        During(InCleaning,
            When(CleaningFinished)
            .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
            .TransitionTo(ReadyForCheckIn)
            .Publish(context => new RoomReadyEvent
            {
                CorrelationId = context.Saga.CorrelationId,
                RoomId = context.Saga.Roomid,
                RoomNumber = context.Saga.RoomNumber
            })
        );

        During(ReadyForCheckIn,
            When(GuestCheckedIn)
            .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
            .TransitionTo(Occupied)
            .Finalize()
        );

        DuringAny(
            When(DamageReported)
            .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
            .TransitionTo(InMaintenance)
            .Send(new Uri("queue:create-maintenance-ticket-queue"), context => new CreateMaintenanceTicketCommand
            {
                CorrelationId = context.Saga.CorrelationId,
                RoomId = context.Saga.Roomid,
                RoomNumber = context.Saga.RoomNumber,
                Description = context.Message.Description
            })
        );

        During(InMaintenance,
            When(MaintenanceResolved)
            .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
            .TransitionTo(AwaitingCleaning)
            .Send(new Uri("queue:assign-cleaning-task-queue"), context => new AssignCleaningTaskCommand
            {
                CorrelationId = context.Saga.CorrelationId,
                RoomId = context.Saga.Roomid,
                RoomNumber = context.Saga.RoomNumber
            })
        );

        SetCompletedWhenFinalized();
    }
}