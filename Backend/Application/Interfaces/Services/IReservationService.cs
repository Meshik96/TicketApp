using Application.DTOs.Reservations;

namespace Application.Interfaces.Services;

public interface IReservationService
{
    Task<ReservationResponse> ReserveSeatsAsync(int userId, Guid seatId);
    Task<bool> ValidateReservationAsync(int userId, Guid seatId);
    Task DeleteReservationAsync(int userId, Guid seatId);
}
