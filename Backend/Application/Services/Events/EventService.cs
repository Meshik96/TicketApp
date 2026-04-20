using Application.DTOs.Events;
using Application.Interfaces.Services;
using Application.Interfaces.Persistence.Events;

namespace Application.Services.Events;

public class EventService : IEventService
{
    private readonly IEventQueries _eventQueries;

    public EventService(IEventQueries eventQueries)
    {
        _eventQueries = eventQueries;
    }

    public async Task<PaginatedEventsResponse> GetPaginatedEventsAsync(int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var (events, totalCount) = await _eventQueries.GetPaginatedActiveEventsAsync(page, pageSize);

        return new PaginatedEventsResponse
        {
            Events = events,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
