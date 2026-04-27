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
        // 1. Validar que todos los asientos seleccionados tengan reserva vigente
        foreach (var seatId in seatIds)
        {
            var reservation = await _reserveQueries.GetReservationAsync(userId, seatId);

            if (reservation == null)
                throw new InvalidOperationException($"No existe o expiró la reserva del asiento {seatId}.");

            // CRITERIO DE ACEPTACIÓN: Validación de tiempo
            if (reservation.ExpiresAt < DateTime.UtcNow)
            {
                // Si expiró, mandamos la orden de liberar el asiento al Command
                await _reservationCommands.DeleteReservationAsync(reservation.ReservationId);
                throw new InvalidOperationException("La reserva ha expirado. Por favor, selecciona tus asientos nuevamente.");
            }
        }

        // 2. Si todas las validaciones pasaron, enviamos el comando para confirmar la compra
        // Este comando internamente debería cambiar el status de 'Reserved' a 'Sold' 
        // y quizás insertar los registros en la tabla de Ventas/Tickets.
        await _seatCommands.ConfirmSeatsPurchaseAsync(userId, seatIds);
    }
}
