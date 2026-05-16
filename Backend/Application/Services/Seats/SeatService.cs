using Application.DTOs.Seats;
using Application.Interfaces.Services;
using Application.Interfaces.Persistence.Seats;
using Application.Interfaces.Persistence.Reservations;

namespace Application.Services.Seats;

public class SeatService : ISeatService
{
    private readonly ISeatQueries _seatQueries;
    private readonly ISeatCommands _seatCommands;
    private readonly IReservationQueries _reserveQueries;
    private readonly IReservationCommands _reservationCommands;

    public SeatService(
        ISeatQueries seatQueries, 
        ISeatCommands seatCommands, 
        IReservationQueries reserveQueries, 
        IReservationCommands reservationCommands)
    {
        _seatQueries = seatQueries;
        _seatCommands = seatCommands;
        _reserveQueries = reserveQueries;
        _reservationCommands = reservationCommands;
    }

    public async Task<List<SeatStateResponse>> GetAllSeatsByEventAsync(int eventId)
    {
        return await _seatQueries.GetAllSeatsByEventAsync(eventId);
    }
    public async Task ConfirmPurchaseAsync(int userId, List<Guid> seatIds)
    {
        await _seatCommands.ConfirmSeatsPurchaseAsync(userId, seatIds);
    }
}
