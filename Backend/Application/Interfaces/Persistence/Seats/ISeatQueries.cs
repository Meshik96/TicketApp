using Application.DTOs.Seats;

namespace Application.Interfaces.Persistence.Seats;

public interface ISeatQueries
{
    Task<List<SeatStateResponse>> GetAllSeatsByEventAsync(int eventId);
}
