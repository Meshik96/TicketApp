using Application.DTOs.Reservations;

namespace Application.Interfaces.Persistence.Reservations;

public interface IReservationCommands
{
    Task<ReservationResponse> ReserveSeatsNaiveAsync(int userId, Guid seatId);
}
