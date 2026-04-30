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

    public async Task<ReservationResponse> ReserveSeatsNaiveAsync(int userId, Guid seatId)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // obtain seat
                var seat = await _context.Seats
                    .Include(s => s.Sector)
                    .FirstOrDefaultAsync(s => s.Id == seatId);

                if (seat == null)
                    throw new InvalidOperationException("Seat not found");

                if (seat.Status != "Available")
                {
                    var auditLog3 = new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Action = "RESERVE_SEAT_FAILED",
                        EntityType = "Seat",
                        EntityId = seatId.ToString(),
                        Details = $"User {userId} failed to reserve seat {seatId}, seat is {seat.Status}",
                        CreatedAt = DateTime.UtcNow
                    };
                    throw new InvalidOperationException($"Seat is already {seat.Status}");
                }
                    

                // verify user exists
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    throw new InvalidOperationException("User not found");

                // change seat status
                seat.Status = "Reserved";
                _context.Seats.Update(seat);

                // create reservation
                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    SeatId = seatId,
                    Status = "Pending",
                    ReservedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5) // 
                };

                await _context.Reservations.AddAsync(reservation);

                // auditlog entries
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Action = "RESERVE_SEAT_SUCCESS",
                    EntityType = "Seat",
                    EntityId = seatId.ToString(),
                    Details = $"Seat: {seat.RowIdentifier}{seat.SeatNumber}, in Sector: {seat.SectorId}, in Event: {seat.Sector.EventId}, reserved by User {userId}",
                    CreatedAt = DateTime.UtcNow
                };
                var auditLog2 = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Action = "CREATE_RESERVATION",
                    EntityType = "Reservation",
                    EntityId = reservation.Id.ToString(),
                    Details = $"Reservation created for Seat: {seat.Id} , by User {userId}",
                    CreatedAt = DateTime.UtcNow
                };
                await _context.AuditLogs.AddAsync(auditLog);
                await _context.AuditLogs.AddAsync(auditLog2);

                // save all changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ReservationResponse
                {
                    ReservationId = reservation.Id,
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
    public async Task DeleteReservationAsync(Guid reservationId)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);
        
        var seat = await _context.Seats
            .FirstOrDefaultAsync(s => s.Id == reservation.SeatId);
       
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = reservation.UserId,
            Action = "RELEASE_SEAT",
            EntityType = "Seat",
            EntityId = reservation.SeatId.ToString(),
            Details = $"Seat {seat.RowIdentifier}{seat.SeatNumber} released manually by User {reservation.UserId}",
            CreatedAt = DateTime.UtcNow
        };
        await _context.AuditLogs.AddAsync(auditLog);
        if (seat != null && seat.Status == "Reserved")
        {
            seat.Status = "Available";
            _context.Seats.Update(seat);
        }
        var auditLog2 = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = reservation.UserId,
            Action = "REMOVE_SEAT_RESERVATION",
            EntityType = "Reservation",
            EntityId = reservation.Id.ToString(),
            Details = $"Reservation removed for Seat {seat.RowIdentifier}{seat.SeatNumber} by User {reservation.UserId}",
            CreatedAt = DateTime.UtcNow
        };
        await _context.AuditLogs.AddAsync(auditLog2);
        _context.Reservations.Remove(reservation);
        
       
        await _context.SaveChangesAsync();
    }
}
