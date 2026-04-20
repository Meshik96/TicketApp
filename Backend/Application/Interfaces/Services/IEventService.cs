using Application.DTOs.Events;

namespace Application.Interfaces.Services;

public interface IEventService
{
    Task<PaginatedEventsResponse> GetPaginatedEventsAsync(int page = 1, int pageSize = 10);
}
