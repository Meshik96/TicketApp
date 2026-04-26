using Application.DTOs.Reservations;
using Application.Interfaces.Persistence.Reservations;
using Domain.Entities;
using Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Reservations;

public class ReservationCommands : IReservationCommands
{
    private readonly AppDbContext _context;

    public ReservationCommands(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReservationResponse> ReserveSeatsNaiveAsync(int userId, Guid seatId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Obtener el asiento
                var seat = await _context.Seats
                    .FirstOrDefaultAsync(s => s.Id == seatId);

                if (seat == null)
                    throw new InvalidOperationException("Seat not found");

                if (seat.Status != "Available")
                    throw new InvalidOperationException($"Seat is already {seat.Status}");

                // Verificar que el usuario existe
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    throw new InvalidOperationException("User not found");

                // Cambiar estado del asiento a "Reserved"
                seat.Status = "Reserved";
                _context.Seats.Update(seat);

                // Crear la reserva
                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    SeatId = seatId,
                    Status = "Pending",
                    ReservedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15) // 15 minutos de reserva
                };

                await _context.Reservations.AddAsync(reservation);

                // Crear entrada en AuditLog
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Action = "RESERVE_SEAT",
                    EntityType = "Seat",
                    EntityId = seatId.ToString(),
                    Details = $"Seat {seat.RowIdentifier}{seat.SeatNumber} in Sector {seat.SectorId} reserved by User {userId}",
                    CreatedAt = DateTime.UtcNow
                };

                await _context.AuditLogs.AddAsync(auditLog);

                // Guardar cambios
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ReservationResponse
                {
                    ReservationId = reservation.Id,
                    Message = "Seat reserved successfully",
                    SeatStatus = "Reserved"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
