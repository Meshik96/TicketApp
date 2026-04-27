using Application.DTOs.Reservations;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.Seats;


namespace API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class TicketController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ISeatService _seatService;
        private readonly IReservationService _reservationService;
        private readonly IUserService _userService;
        private readonly ISeatService _seatsService;

        public TicketController(
            IEventService eventService,
            ISeatService seatService,
            IReservationService reservationService,
            IUserService userService,
            ISeatService seatsService)
        {
            _eventService = eventService;
            _seatService = seatService;
            _reservationService = reservationService;
            _userService = userService;
            _seatsService = seatsService;
        }

        /// <summary>
        /// GET: Obtiene un listado paginado del catálogo de eventos activos
        /// </summary>
        /// <param name="page">Número de página (default: 1)</param>
        /// <param name="pageSize">Cantidad de eventos por página (default: 10)</param>
        [HttpGet("events")]
        public async Task<IActionResult> GetPaginatedEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _eventService.GetPaginatedEventsAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// GET: Retorna el estado actual de todas las butacas (Disponibles, Reservadas, Vendidas)
        /// para renderizar el plano de asientos
        /// </summary>
        /// <param name="eventId">ID del evento</param>
        [HttpGet("events/{eventId}/seats")]
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
        /// POST: Versión "naive" que acepta un intento de reserva, cambia el estado de la butaca
        /// y persiste el evento en AuditLog (sin control estricto de concurrencia)
        /// </summary>
        [HttpPost("reservations")]
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
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        [HttpDelete("reservations")]
        public async Task<IActionResult> DeleteReservation([FromQuery]Guid seatId, [FromQuery] int userId)
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

        [HttpPost("buy")]
        public async Task<IActionResult> BuySeats([FromBody] BuyRequest request)
        {
            // 1. Validar fallo de serialización del JSON
            if (request == null)
                return BadRequest("El objeto request es nulo. Falló el binding del JSON desde el frontend.");

            if (request.SeatIds == null)
                return BadRequest("La lista SeatIds llegó nula.");

            try
            {
                await _seatsService.ConfirmPurchaseAsync(request.UserId, request.SeatIds);
                return Ok(new { message = "Compra realizada con éxito" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Enviar el StackTrace revela la clase y línea exacta del error
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            var created = await _userService.CreateAsync(user);
            return Ok(created);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            var result = await _userService.UpdateAsync(id, user);

            if (!result)
                return NotFound();

            return NoContent();
        }


    }
}
