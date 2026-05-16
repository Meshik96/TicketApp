using Application.DTOs.Reservations;
using Application.Interfaces.Services;
using Application.Interfaces.Persistence.Reservations;

namespace Application.Services.Reservations;

public class ReservationService : IReservationService
{
    private readonly IReservationCommands _reservationCommands;
    private readonly IReservationQueries _reservationQueries;

    public ReservationService(IReservationCommands reservationCommands, IReservationQueries reservationQueries)
    {
        _reservationCommands = reservationCommands;
        _reservationQueries = reservationQueries;
    }

    public async Task<ReservationResponse> ReserveSeatsAsync(int userId, Guid seatId)
    {
        return await _reservationCommands.ReserveSeatsAsync(userId, seatId);
    }
    public async Task<bool> ValidateReservationAsync(int userId, Guid seatId)
    {
        var reservation = await _reservationQueries.GetReservationAsync(userId, seatId);

        if (reservation == null) return false;

        // VALIDACIÓN DE EXPIRACIÓN
        if (reservation.ExpiresAt < DateTime.UtcNow)
        {
            await _reservationCommands.DeleteReservationAsync(reservation.ReservationId);
            return false;
        }

        return true;
    }
    public async Task DeleteReservationAsync(int userId, Guid seatId)
    {
        var reservation = await _reservationQueries.GetReservationAsync(userId, seatId);

        // Validar existencia y estado de la reserva
        if (reservation == null || reservation.Status != "Pending")
        {
            throw new InvalidOperationException("No se encontró una reserva activa para cancelar.");
        }

        await _reservationCommands.DeleteReservationAsync(reservation.ReservationId);
    }
    public async Task<List<UserReservationResponse>> GetUserReservationsAsync(int userId)
    {
        // Aquí puede agregar lógica de validación adicional si fuera necesario
        return await _reservationQueries.GetUserReservationsAsync(userId);
    }
}