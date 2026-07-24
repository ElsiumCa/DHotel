using Microsoft.EntityFrameworkCore;
using FrontDesk.API.Entities;
using MassTransit;

namespace FrontDesk.API.Data
{
    public class FrontDeskDbContext : DbContext
    {
        public FrontDeskDbContext(DbContextOptions<FrontDeskDbContext> options) : base(options)
        {
        }
        public DbSet<Guest> Guests => Set<Guest>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddTransactionalOutboxEntities();
    }
    }
}