using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infraestructure.Data.Configurations;

namespace Infraestructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar las configuraciones con datos de seed
            modelBuilder.ApplyConfiguration(new EventConfiguration());
            modelBuilder.ApplyConfiguration(new SectorConfiguration());
            modelBuilder.ApplyConfiguration(new SeatConfiguration());

            // Configuración de precisión para decimales (Importante para Price)
            modelBuilder.Entity<Sector>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            // Relación Event -> Sector (1:N)
            modelBuilder.Entity<Sector>()
                .HasOne(s => s.Event)
                .WithMany(e => e.Sectors)
                .HasForeignKey(s => s.EventId);

            // Relación Sector -> Seat (1:N)
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Sector)
                .WithMany(sc => sc.Seats)
                .HasForeignKey(s => s.SectorId);

            // Relación Seat -> Reservation (1:1)
            // Según el diagrama, un asiento se asigna a una reserva
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Seat)
                .WithOne(s => s.Reservation)
                .HasForeignKey<Reservation>(r => r.SeatId);

            // Relación User -> Reservation (1:N)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId);

            // Relación User -> AuditLog (1:N)
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull); // Si se borra el usuario, el log queda
        }
    }
}
