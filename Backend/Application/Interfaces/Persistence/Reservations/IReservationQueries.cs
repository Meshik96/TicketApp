using Application.DTOs.Reservations;

namespace Application.Interfaces.Persistence.Reservations
{
    public interface IReservationQueries
    {
        Task<ReservationResponse> GetReservationAsync(int userId, Guid seatId);
    }
}
