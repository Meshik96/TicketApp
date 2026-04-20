using Application.DTOs.Seats;
using Application.Interfaces.Services;
using Application.Interfaces.Persistence.Seats;

namespace Application.Services.Seats;

public class SeatService : ISeatService
{
    private readonly ISeatQueries _seatQueries;

    public SeatService(ISeatQueries seatQueries)
    {
        _seatQueries = seatQueries;
    }

    public async Task<List<SeatStateResponse>> GetAllSeatsByEventAsync(int eventId)
    {
        return await _seatQueries.GetAllSeatsByEventAsync(eventId);
    }
}
