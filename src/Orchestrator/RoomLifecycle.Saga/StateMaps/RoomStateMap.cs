using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace RoomLifecycle.Saga.StateMaps
{
    public class RoomStateMap : SagaClassMap<RoomStateData>
    {
        protected override void Configure(EntityTypeBuilder<RoomStateData> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.RoomNumber).HasMaxLength(32);
        }
    }
}