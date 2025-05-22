using Microsoft.EntityFrameworkCore;

namespace Energycom.Ingestion.Data;

public class ECOMDbContext(DbContextOptions<ECOMDbContext> options) : DbContext(options)
{
    public DbSet<Entities.Group> Groups { get; set; }
    public DbSet<Entities.Meter> Meters { get; set; }
    public DbSet<Entities.Reading> Readings { get; set; }
    
    public DbSet<Entities.Site> Sites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.Site>().OwnsOne(e => e.Grid, b =>
        {
            b.Property(e => e.Value).HasColumnName("Grid");
        });
    }
}