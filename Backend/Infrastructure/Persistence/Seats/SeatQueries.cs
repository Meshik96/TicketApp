using Application.DTOs.Seats;
using Application.Interfaces.Persistence.Seats;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seats;

public class SeatQueries : ISeatQueries
{
    private readonly AppDbContext _context;

    public SeatQueries(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SeatStateResponse>> GetAllSeatsByEventAsync(int eventId)
    {
        var seats = await _context.Seats
            .AsNoTracking()
            .Where(s => s.Sector.EventId == eventId)
            .Include(s => s.Sector)
            .Include(s => s.Reservations)
            .OrderBy(s => s.Sector.Id)
            .ThenBy(s => s.RowIdentifier)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync();

        var result = seats.Select(s => 
        {
            var activeReservation = s.Reservations
                .Where(r => r.Status == "Pending")
                .FirstOrDefault();

            return new SeatStateResponse
            {
                Id = s.Id,
                SectorId = s.SectorId,
                SectorName = s.Sector.Name,
                UserId = activeReservation?.UserId ?? 0,
                ExpiresAt = activeReservation?.ExpiresAt ?? DateTime.MinValue,
                RowIdentifier = s.RowIdentifier,
                SeatNumber = s.SeatNumber,
                Status = s.Status,
                Price = s.Sector.Price,
                SectorGridX = s.Sector.GridX,
                SectorGridY = s.Sector.GridY
            };
        }).ToList();

        return result;
    }
}
