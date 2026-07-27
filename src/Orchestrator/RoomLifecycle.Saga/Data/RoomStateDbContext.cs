using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using RoomLifecycle.Saga.StateMaps;

namespace RoomLifecycle.Saga.Data
{
    public class RoomStateDbContext : SagaDbContext
    {
        public RoomStateDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get
            {
                yield return new RoomStateMap();
            }
        }
    }
}