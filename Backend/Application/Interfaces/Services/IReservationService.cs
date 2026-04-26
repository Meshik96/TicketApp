using Application.DTOs.Reservations;

namespace Application.Interfaces.Services;

public interface IReservationService
{
    Task<ReservationResponse> ReserveSeatsNaiveAsync(int userId, Guid seatId);
}
