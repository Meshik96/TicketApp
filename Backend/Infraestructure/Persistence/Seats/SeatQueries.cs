using Application.DTOs.Seats;
using Application.Interfaces.Persistence.Seats;
using Infraestructure.Data;
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
            .OrderBy(s => s.Sector.Id)
            .ThenBy(s => s.RowIdentifier)
            .ThenBy(s => s.SeatNumber)
            .Select(s => new SeatStateResponse
            {
                Id = s.Id,
                SectorId = s.SectorId,
                SectorName = s.Sector.Name,
                RowIdentifier = s.RowIdentifier,
                SeatNumber = s.SeatNumber,
                Status = s.Status
            })
            .ToListAsync();

        return seats;
    }
}
