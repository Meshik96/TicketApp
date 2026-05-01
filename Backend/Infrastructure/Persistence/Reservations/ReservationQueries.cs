using Application.DTOs.Reservations;
using Application.Interfaces.Persistence.Reservations;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Reservations
{
    public class ReservationQueries : IReservationQueries
    {
        private readonly AppDbContext _context;
        public ReservationQueries(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ReservationResponse> GetReservationAsync(int userId, Guid seatId)
        {
            var reservation = await _context.Reservations
                .Where(r => r.UserId == userId && r.SeatId == seatId)
                .Select(r => new ReservationResponse
                {
                    ReservationId = r.Id,
                    UserId = r.UserId,
                    SeatId = r.SeatId,
                    SeatStatus = r.Status,
                    ReservedAt = r.ReservedAt,
                    ExpiresAt = r.ExpiresAt
                })
                .FirstOrDefaultAsync();

            return reservation;
        }
    }
}
