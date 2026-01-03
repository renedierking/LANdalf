using API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;

namespace API.Data {
    public class AppDbContext : DbContext {
        public DbSet<PcDevice> PcDevices => Set<PcDevice>();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<PcDevice>()
                .Property(p => p.MacAddress)
                .HasConversion(
                    mac => mac.ToString(),                    // DB: "AABBCCDDEEFF"
                    value => PhysicalAddress.Parse(value)     // Domain
                );

            modelBuilder.Entity<PcDevice>()
                .Property(p => p.IpAddress)
                .HasConversion(
                    ip => ip.ToString(),                     // "192.168.1.10"
                    value => IPAddress.Parse(value)
                );

            modelBuilder.Entity<PcDevice>()
                .Property(p => p.BroadcastAddress)
                .HasConversion(
                    ip => ip.ToString(),
                    value => IPAddress.Parse(value)
                );
        }
    }
}
