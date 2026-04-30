using Application.DTOs.Reservations;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/reservations")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        /// <summary>
        /// POST: Versión "naive" que acepta un intento de reserva, cambia el estado de la butaca
        /// y persiste el evento en AuditLog (sin control estricto de concurrencia)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReserveSeats([FromBody] ReservationRequest request)
        {
            try
            {
                if (request.UserId <= 0)
                    return BadRequest(new { message = "Invalid UserId" });

                if (request.SeatId == Guid.Empty)
                    return BadRequest(new { message = "Invalid SeatId" });

                var result = await _reservationService.ReserveSeatsNaiveAsync(request.UserId, request.SeatId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: Elimina una reserva existente
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteReservation([FromQuery] Guid seatId, [FromQuery] int userId)
        {
            try
            {
                await _reservationService.DeleteReservationAsync(userId, seatId);
                return Ok(new { message = "Reservation deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }
    }
}
