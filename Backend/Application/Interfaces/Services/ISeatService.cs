using Application.DTOs.Seats;

namespace Application.Interfaces.Services;

public interface ISeatService
{
    Task<List<SeatStateResponse>> GetAllSeatsByEventAsync(int eventId);
}
