using Application.DTOs.Seats;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/seats")]
    public class SeatController : ControllerBase
    {
        private readonly ISeatService _seatService;

        public SeatController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        /// <summary>
        /// GET: Retorna el estado actual de todas las butacas (Disponibles, Reservadas, Vendidas)
        /// para renderizar el plano de asientos
        /// </summary>
        /// <param name="eventId">ID del evento</param>
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetSeatMap(int eventId)
        {
            try
            {
                var seats = await _seatService.GetAllSeatsByEventAsync(eventId);
                if (!seats.Any())
                    return NotFound(new { message = "Event not found or has no seats" });

                return Ok(new { eventId, seats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// POST: Confirma la compra de butacas
        /// </summary>
        [HttpPost("purchase")]
        public async Task<IActionResult> BuySeats([FromBody] BuyRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "El objeto request es nulo. Falló el binding del JSON desde el frontend." });

            if (request.SeatIds == null)
                return BadRequest(new { message = "La lista SeatIds llegó nula." });

            try
            {
                await _seatService.ConfirmPurchaseAsync(request.UserId, request.SeatIds);
                return Ok(new { message = "Compra realizada con éxito" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
