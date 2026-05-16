using Application.DTOs.Reservations;
using Application.Interfaces.Persistence.Reservations;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Reservations;

public class ReservationCommands : IReservationCommands
{
    private readonly AppDbContext _context;

    public ReservationCommands(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReservationResponse> ReserveSeatsAsync(int userId, Guid seatId)
    {
        try
        {
            var seat = await _context.Seats
                .Include(s => s.Sector)
                .Include(s => s.Sector.Event)
                .FirstOrDefaultAsync(s => s.Id == seatId);

            if (seat == null)
                throw new KeyNotFoundException("Seat not found");

            if (seat.Status != "Available")
            {
                var expiredReservation = await _context.Reservations
                    .Where(r => r.SeatId == seatId && r.ExpiresAt < DateTime.UtcNow && r.Status == "Pending")
                    .FirstOrDefaultAsync();
                if (expiredReservation != null) //JIT release 
                {
                    // Cambiar estado de la reserva expirada a "Expired"
                    expiredReservation.Status = "Expired";
                    seat.Status = "Available";
                    seat.Version++;
                    await _context.AuditLogs.AddAsync(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        UserId = null,
                        SeatId = seatId,
                        ReservationId = expiredReservation.Id,
                        Action = "RELEASE_SEAT_JIT",
                        Details = $"Asiento liberado JIT durante nueva reserva.",
                        CreatedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
                else //reservation failed
                {
                    await LogFailureAsync(userId, seatId, $"Seat is already {seat.Status}");
                    throw new InvalidOperationException("SEAT_UNAVAILABLE");
                }
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new KeyNotFoundException("User not found");

            var eventId = seat.Sector.EventId;
            var existingExpirationTime = await _context.Reservations
                .Where(r => r.UserId == userId
                         && r.Seat.Sector.EventId == eventId
                         && r.Status == "Pending"
                         && r.ExpiresAt > DateTime.UtcNow)
                .Select(r => (DateTime?)r.ExpiresAt)
                .FirstOrDefaultAsync();
            DateTime expirationDate = existingExpirationTime ?? DateTime.UtcNow.AddMinutes(5);

            seat.Status = "Reserved";
            seat.Version++;
            
            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SeatId = seatId,
                Status = "Pending",
                ReservedAt = DateTime.UtcNow,
                ExpiresAt = expirationDate
            };

            await _context.Reservations.AddAsync(reservation);

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SeatId = seatId,
                ReservationId = reservation.Id,
                Action = "RESERVE_SEAT_SUCCESS",
                Details = $"Seat: {seat.RowIdentifier}{seat.SeatNumber}, Sector: {seat.SectorId}, Event: {seat.Sector.EventId}, reserved by User: {userId}",
                CreatedAt = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            return new ReservationResponse
            {
                ReservationId = reservation.Id,
                Status = "Reserved"
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            await LogFailureAsync(userId, seatId, "Concurrency conflict: Seat was modified by another transaction");
            throw new InvalidOperationException("CONCURRENCY_CONFLICT");
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true || ex.InnerException?.Message.Contains("IX_Reservations_SeatId") == true)
        {
            await LogFailureAsync(userId, seatId, "Concurrency conflict: Another reservation for this seat already exists");
            throw new InvalidOperationException("CONCURRENCY_CONFLICT");
        }
    }

    private async Task LogFailureAsync(int userId, Guid seatId, string details)
    {
        _context.ChangeTracker.Clear();

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SeatId = seatId,
            ReservationId = null,
            Action = "RESERVE_SEAT_FAILED",
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteReservationAsync(Guid reservationId)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);
        
        var seat = await _context.Seats
            .Include(s => s.Sector)
            .FirstOrDefaultAsync(s => s.Id == reservation.SeatId);
       
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = reservation.UserId,
            SeatId = reservation.SeatId,
            ReservationId = reservation.Id,
            Action = "REMOVE_SEAT_RESERVATION",
            Details = $"Seat {seat.RowIdentifier}{seat.SeatNumber}, Sector: {seat.SectorId}, Event: {seat.Sector.EventId} released manually by User: {reservation.UserId}",
            CreatedAt = DateTime.UtcNow
        };
        await _context.AuditLogs.AddAsync(auditLog);
        if (seat != null && seat.Status == "Reserved")
        {
            seat.Status = "Available";
            seat.Version++;
        }

        await _context.Reservations
            .Where(r => r.Id == reservationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.Status, "Expired")
                .SetProperty(r => r.ExpiresAt, DateTime.UtcNow));


        await _context.SaveChangesAsync();
    }
}
