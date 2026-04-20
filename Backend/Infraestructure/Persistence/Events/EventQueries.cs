using Application.DTOs.Events;
using Application.Interfaces.Persistence.Events;
using Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Events;

public class EventQueries : IEventQueries
{
    private readonly AppDbContext _context;

    public EventQueries(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<EventListResponse> Events, int TotalCount)> GetPaginatedActiveEventsAsync(int page, int pageSize)
    {
        var query = _context.Events
            .AsNoTracking()
            .Where(e => e.Status == "Active");

        var totalCount = await query.CountAsync();

        var events = await query
            .OrderByDescending(e => e.EventDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventListResponse
            {
                Id = e.Id,
                Name = e.Name,
                EventDate = e.EventDate,
                Venue = e.Venue,
                Status = e.Status
            })
            .ToListAsync();

        return (events, totalCount);
    }
}
