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
            if (seatIds == null || !seatIds.Any())
                throw new InvalidOperationException("No se enviaron asientos para procesar.");

            try
            {
                var pendingReservations = await _context.Reservations
                    .Where(r => seatIds.Contains(r.SeatId)
                             && r.UserId == userId
                             && r.Status == "Pending")
                    .ToListAsync();

                if (pendingReservations.Count != seatIds.Count)
                {
                    throw new InvalidOperationException("La reserva ha expirado. Por favor, selecciona tus asientos nuevamente.");
                }

                var seats = await _context.Seats
                    .Where(s => seatIds.Contains(s.Id))
                    .ToListAsync();

                var auditLogs = new List<AuditLog>();

                foreach (var seat in seats)
                {
                    seat.Status = "Sold";
                    seat.Version++;

                    var reservation = pendingReservations.First(r => r.SeatId == seat.Id);
                    reservation.Status = "Paid";

                    auditLogs.Add(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        SeatId = seat.Id,
                        ReservationId = reservation.Id,
                        Action = "BUY_SEAT_SUCCESS",
                        Details = $"Seat {seat.RowIdentifier}{seat.SeatNumber} in Sector {seat.SectorId} bought by User {userId}",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                _context.Seats.UpdateRange(seats);
                _context.Reservations.UpdateRange(pendingReservations);
                await _context.AuditLogs.AddRangeAsync(auditLogs);

                // La transacción implícita se maneja aquí
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Limpiar el estado del contexto actual
                _context.ChangeTracker.Clear();

                var errorLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Action = "BUY_SEAT_ERROR",
                    Details = $"Error confirmando compra: {ex.Message}",
                    CreatedAt = DateTime.UtcNow
                };

                await _context.AuditLogs.AddAsync(errorLog);
                await _context.SaveChangesAsync();

                throw;
            }
        }

        public async Task DeleteExpiredReservationsAsync()
        {
            var now = DateTime.UtcNow;
            const int systemUserId = 0; // ID reservado para procesos automáticos

            // 1. query
            var expiredReservations = await _context.Reservations
                .Where(r => r.ExpiresAt < now && r.Status == "Pending")
                .ToListAsync();

            if (!expiredReservations.Any()) return;

            var seatIds = expiredReservations.Select(x => x.SeatId).ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. update seats
                await _context.Seats
                    .Where(s => seatIds.Contains(s.Id))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Status, "Available"));

                // 3. update reservations to "Expired"
                await _context.Reservations
                    .Where(r => r.ExpiresAt < now && r.Status == "Pending")
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(r => r.Status, "Expired"));

                // 4. audit
                var auditLogs = new List<AuditLog>();

                foreach (var reservation in expiredReservations)
                {
                    auditLogs.Add(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        UserId = null,
                        SeatId = reservation.SeatId,
                        ReservationId = reservation.Id,
                        Action = "RELEASE_SEAT",
                        Details = "Asiento liberado por expiración de reserva.",
                        CreatedAt = DateTime.UtcNow
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