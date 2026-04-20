using Application.DTOs.Events;

namespace Application.Interfaces.Persistence.Events;

public interface IEventQueries
{
    Task<(List<EventListResponse> Events, int TotalCount)> GetPaginatedActiveEventsAsync(int page, int pageSize);
}
