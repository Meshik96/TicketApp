using Application.DTOs.Reservations;
using Application.Interfaces.Services;
using Application.Interfaces.Persistence.Reservations;

namespace Application.Services.Reservations;

public class ReservationService : IReservationService
{
    private readonly IReservationCommands _reservationCommands;

    public ReservationService(IReservationCommands reservationCommands)
    {
        _reservationCommands = reservationCommands;
    }

    public async Task<SimpleReservationResponse> ReserveSeatsNaiveAsync(int userId, Guid seatId)
    {
        return await _reservationCommands.ReserveSeatsNaiveAsync(userId, seatId);
    }
}
