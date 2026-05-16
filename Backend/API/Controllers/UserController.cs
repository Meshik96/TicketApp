using Application.DTOs.Users;
using Application.Interfaces.Services;
using Application.Services.Reservations;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IReservationService _reservationService;

        public UserController(IUserService userService, IReservationService reservationService)
        {
            _userService = userService;
            _reservationService = reservationService;
        }

        /// <summary>
        /// GET: Obtiene todos los usuarios
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                var userDtos = users.Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                }).ToList();

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// GET: Obtiene un usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);

                if (user == null)
                    return NotFound(new { message = "User not found" });

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// POST: Crea un nuevo usuario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null)
                return BadRequest(new { message = "Invalid user data" });

            try
            {
                var user = new Domain.Entities.User
                {
                    Name = createUserDto.Name,
                    Email = createUserDto.Email,
                    PasswordHash = createUserDto.Password
                };

                var created = await _userService.CreateAsync(user);

                var userDto = new UserDto
                {
                    Id = created.Id,
                    Name = created.Name,
                    Email = created.Email
                };

                return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        /// <summary>
        /// PUT: Actualiza un usuario existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (updateUserDto == null)
                return BadRequest(new { message = "Invalid user data" });

            try
            {
                var user = new Domain.Entities.User
                {
                    Id = id,
                    Name = updateUserDto.Name,
                    Email = updateUserDto.Email
                };

                var result = await _userService.UpdateAsync(id, user);

                if (!result)
                    return NotFound(new { message = "User not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        [HttpGet("{userId}/reservations")]
        public async Task<IActionResult> GetUserReservations(int userId)
        {
            try
            {
                var reservations = await _reservationService.GetUserReservationsAsync(userId);
                return Ok(new { reservations });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las reservas", details = ex.Message });
            }
        }
    }
}
