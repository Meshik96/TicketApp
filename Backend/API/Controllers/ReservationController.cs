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

        [HttpPost]
        public async Task<IActionResult> ReserveSeat([FromBody] ReservationRequest request)
        {
            try
            {
                var response = await _reservationService.ReserveSeatsAsync(request.UserId, request.SeatId);
                return StatusCode(201, response);
            }
            catch (InvalidOperationException ex) when (ex.Message is "SEAT_UNAVAILABLE" or "CONCURRENCY_CONFLICT")
            {
                return Conflict(new { error = "El asiento ya se encuentra reservado o no está disponible." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor." });
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
