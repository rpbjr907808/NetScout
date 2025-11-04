using Microsoft.EntityFrameworkCore;
using NetScout.Models;

namespace NetScout.Data;

public class NetworkDbContext : DbContext
{
    public DbSet<NetworkDevice> NetworkDevices { get; set; }
    public DbSet<OpenPort> OpenPorts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=netscout.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NetworkDevice>()
            .HasIndex(d => d.IpAddress)
            .IsUnique();

        modelBuilder.Entity<NetworkDevice>()
            .HasMany(d => d.OpenPorts)
            .WithOne(p => p.NetworkDevice)
            .HasForeignKey(p => p.NetworkDeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
