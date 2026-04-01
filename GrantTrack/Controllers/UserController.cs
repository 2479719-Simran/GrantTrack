using GrantTrack.Dto.User;
using GrantTrack.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registers a new user with the default Applicant role.
        /// </summary>
        /// <param name="dto">User registration details.</param>
        /// <returns>Returns success status after user creation.</returns>

        [HttpPost("registeruser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // [Authorize(Roles = "Admin")] // Only Admin can register new users
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto dto)
        {
            // Model validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _userService.RegisterUserAsync(dto);
                return StatusCode(StatusCodes.Status201Created, "User created successfully");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred"
                );
            }
        }
    }
}
