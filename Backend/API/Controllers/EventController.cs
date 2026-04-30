using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// GET: Obtiene un listado paginado del catálogo de eventos activos
        /// </summary>
        /// <param name="page">Número de página (default: 1)</param>
        /// <param name="pageSize">Cantidad de eventos por página (default: 10)</param>
        [HttpGet]
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
    }
}
