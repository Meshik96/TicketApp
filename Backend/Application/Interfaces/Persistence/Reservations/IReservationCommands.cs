using Application.DTOs.Reservations;

namespace Application.Interfaces.Persistence.Reservations;

public interface IReservationCommands
{
    Task<ReservationResponse> ReserveSeatsAsync(int userId, Guid seatId);
    Task DeleteReservationAsync(Guid reservationId);
}
