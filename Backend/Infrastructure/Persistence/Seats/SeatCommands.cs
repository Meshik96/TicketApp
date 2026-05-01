using Domain.Entities;
using Infrastructure.Data;
using Application.Interfaces.Persistence.Seats;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seats
{
    public class SeatCommands : ISeatCommands
    {
        private readonly AppDbContext _context;

        public SeatCommands(AppDbContext context)
        {
            _context = context;
        }
        public async Task ConfirmSeatsPurchaseAsync(int userId, List<Guid> seatIds)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Cambiar estado de los asientos a "Sold"
                    var seats = await _context.Seats
                        .Where(s => seatIds.Contains(s.Id))
                        .Include(s => s.Sector)
                        .Include(r => r.Reservation)
                        .ToListAsync();
                    foreach (var seat in seats)
                    {
                        seat.Status = "Sold";
                        var auditlog = new AuditLog
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            Action = "REMOVE_SEAT_RESERVATION(SOLD)",
                            EntityType = "Reservation",
                            EntityId = seat.Reservation.Id.ToString(),
                            Details = $"Reservation removed for Seat {seat.RowIdentifier}{seat.SeatNumber} by User {userId} (Sold)",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.AuditLogs.Add(auditlog);
                        _context.Reservations.Remove(seat.Reservation);
                        var auditLog2 = new AuditLog
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            Action = "BUY_SEAT",
                            EntityType = "Seat",
                            EntityId = seat.Id.ToString(),
                            Details = $"Seat {seat.RowIdentifier}{seat.SeatNumber} in Sector {seat.SectorId} in event {seat.Sector.EventId} bought by User {userId}",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.AuditLogs.Add(auditLog2);
                    }
                    _context.Seats.UpdateRange(seats);
                    // Aquí podrías agregar lógica para insertar registros en una tabla de Ventas/Tickets
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }

            }
        }
        public async Task DeleteExpiredReservationsAsync()
        {
            var now = DateTime.UtcNow;
            const int systemUserId = 0; // ID reservado para procesos automáticos

            // 1. Identificar reservas y asientos afectados
            var expiredData = await _context.Reservations
                .Where(r => r.ExpiresAt < now)
                .Select(r => new { r.Id, r.SeatId })
                .ToListAsync();

            if (!expiredData.Any()) return;

            var seatIds = expiredData.Select(x => x.SeatId).ToList();
            var reservationIds = expiredData.Select(x => x.Id).ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Actualización masiva de asientos
                await _context.Seats
                    .Where(s => seatIds.Contains(s.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Status, "Available"));

                // 3. Eliminación masiva de reservas
                await _context.Reservations
                    .Where(r => reservationIds.Contains(r.Id))
                    .ExecuteDeleteAsync();

                // 4. Auditoría
                var auditLogs = new List<AuditLog>();

                foreach (var data in expiredData)
                {
                    // Log para la Reserva
                    auditLogs.Add(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        UserId = null,
                        Action = "EXPIRE_RESERVATION",
                        EntityType = "Reservation",
                        EntityId = data.Id.ToString(),
                        Details = "Reserva expirada automáticamente por el sistema.",
                        CreatedAt = now
                    });

                    // Log para el Asiento
                    auditLogs.Add(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        UserId = null,
                        Action = "RELEASE_SEAT",
                        EntityType = "Seat",
                        EntityId = data.SeatId.ToString(),
                        Details = "Asiento liberado por expiración de reserva.",
                        CreatedAt = now
                    });
                }

                await _context.AuditLogs.AddRangeAsync(auditLogs);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}