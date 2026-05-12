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

                        // Cambiar estado de la reserva a "Paid" en lugar de eliminarla
                        if (seat.Reservation != null)
                        {
                            seat.Reservation.Status = "Paid";
                        }

                        var auditLog = new AuditLog
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            SeatId = seat.Id,
                            ReservationId = seat.Reservation?.Id,
                            Action = "BUY_SEAT",
                            Details = $"Seat {seat.RowIdentifier}{seat.SeatNumber} in Sector {seat.SectorId} in event {seat.Sector.EventId} bought by User {userId}",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.AuditLogs.Add(auditLog);
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