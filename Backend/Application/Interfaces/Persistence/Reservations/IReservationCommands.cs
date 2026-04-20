using Application.DTOs.Reservations;

namespace Application.Interfaces.Persistence.Reservations;

public interface IReservationCommands
{
    Task<SimpleReservationResponse> ReserveSeatsNaiveAsync(int userId, Guid seatId);
}
