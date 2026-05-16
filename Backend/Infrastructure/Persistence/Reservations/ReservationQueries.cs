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
                .Where(r => r.UserId == userId && r.SeatId == seatId && r.Status == "Pending") // Filtro obligatorio
                .FirstOrDefaultAsync();

            if (reservation == null) return null;

            return new ReservationResponse
            {
                ReservationId = reservation.Id,
                Status = reservation.Status
            };
        }
        public async Task<List<UserReservationResponse>> GetUserReservationsAsync(int userId)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Where(r => r.UserId == userId && r.Status == "Paid")
                .Select(r => new UserReservationResponse
                {
                    BookingId = r.Id,
                    Status = "purchased", 
                    EventId = r.Seat.Sector.EventId,
                    EventName = r.Seat.Sector.Event.Name,
                    EventDate = r.Seat.Sector.Event.EventDate,
                    VenueName = r.Seat.Sector.Event.Venue,
                    SectorName = r.Seat.Sector.Name,
                    RowIdentifier = r.Seat.RowIdentifier,
                    SeatNumber = r.Seat.SeatNumber,
                    Price = r.Seat.Sector.Price
                })
                .ToListAsync();
        }
    }
}
