using System.Net;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto;
using GrantTrack.Dto.LoginDtos;
using GrantTrack.Dto.User;
using GrantTrack.Dto.UserDtos;
using GrantTrack.Dto.UserDTOs;
using GrantTrack.Service.AuthServices;
using GrantTrack.Service.Interfaces;
using GrantTrack.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly GrantTrackDbContext _context;
        private readonly IConfiguration _config;
        /// <summary>
        /// purpose: The UserController is responsible for handling user-related operations such as authentication, registration, and profile updates. It provides endpoints for users to log in, register, and manage their accounts.
        /// </summary>
        /// <param name="userService">The user service instance</param>
        /// <param name="authService">The authentication service instance</param>
        /// <param name="context">The database context instance</param>
        /// <param name="config">The configuration instance</param>
        public UserController(IUserService userService, IAuthService authService, GrantTrackDbContext context, IConfiguration config)
        {
            _userService = userService;
            _authService = authService;
            _context = context;
            _config = config;
        }

        /// <summary>
        /// purpose: to authenticate users and provide them with a JWT token for subsequent requests.
        /// </summary>
        /// <param name="loginRequest">The login request DTO</param>
        /// <returns></returns>
        [HttpPost("login")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            var loginResponse = await _userService.LoginAsync(loginRequest, _context, _config);
            // If login fails, return 401 Unauthorized with error message
            if (!loginResponse.Success)
            {
                if (loginResponse.ErrorMessage == "Email or Password cannot be empty")
                {
                    return BadRequest(new { error = loginResponse.ErrorMessage }); //400 BadRequest
                }
                return Unauthorized(new { error = loginResponse.ErrorMessage }); //401 Unauthorized
            }
            // If login is successful, return 200 OK with the JWT token    
            return Ok(loginResponse.AccessToken);
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
        [HttpPost("update/{id:int}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }
            try
            {
                var res = await _userService.UpdateUser(id, request);
                if (res == null)
                {
                    return NotFound("UserId doest not exist");
                }
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Forgot password — POST /api/v1/user/forgotpassword
        /// </summary>
        [HttpPost("forgotpassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UserForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (model == null)
                return BadRequest(Messages.InvalidRequest);

            var (success, message) = await _authService.ForgotPasswordAsync(model);

            if (!success)
                return BadRequest(message);

            return Ok(new { message });
        }

        /// <summary>
        /// Retrieves all users for administrative review. Restricted to Admins.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<IEnumerable<ViewUserDto>>> GetAll()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync(_context);
                return Ok(users);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving the user list.");
            }
        }

        /// <summary>
        /// As an admin, change the status to inactive (Soft Delete) for a specific user ID.
        /// Route: PATCH /api/v1/user/delete-user/5
        /// </summary>
        [HttpPatch("delete-user/{id:int}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> SoftDeleteUser([FromRoute] int id)
        {
            try
            {
                var result = await _userService.DeactivateUserByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new { message = Messages.UserNotFoundById });
                }

                if (result == "ALREADY_INACTIVE")
                {
                    return BadRequest(new { message = Messages.UserAlreadyInactive });
                }
                return Ok(new
                {
                    message = Messages.UserDeactivated
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = Messages.SomethingWentWrong });
            }
        }

    }
}
