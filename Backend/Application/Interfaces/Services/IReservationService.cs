using Application.DTOs.Reservations;

namespace Application.Interfaces.Services;

public interface IReservationService
{
    Task<SimpleReservationResponse> ReserveSeatsNaiveAsync(int userId, Guid seatId);
}
