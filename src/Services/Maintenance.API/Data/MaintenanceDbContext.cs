using Maintenance.API.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.API.Data;

public class MaintenanceDbContext : DbContext
{

public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options) : base(options)
{
}

    public DbSet<MaintenanceTicket> maintenanceTickets => Set<MaintenanceTicket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddTransactionalOutboxEntities();
    }
}