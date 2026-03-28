using GrantTrack.Dto.User;
using GrantTrack.Dto.UserDtos;
using GrantTrack.Service.AuthServices;
using GrantTrack.Service.Interfaces;
using GrantTrack.Utility;
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
        public UserController(IUserService userService , IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user with the default Applicant role.
        /// </summary>
        /// <param name="dto">User registration details.</param>
        /// <returns>Returns success status after user creation.</re

        [HttpPost("registeruser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        [HttpPost]
        [Route("{id:int}")] 
        public async Task<IActionResult> UpdateUser([FromRoute] int id , [FromBody] UpdateUserRequestDto request)
        {
            if(request == null)
            {
                return BadRequest("Request cannot be null");
            }
            try
            {
                var res = await _userService.UpdateUser(id , request);
                if(res == null)
                {
                    return NotFound("User not found");
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
            if(model == null)
                return BadRequest(Messages.InvalidRequest);
        
            var(success,message) = await _authService.ForgotPasswordAsync(model);
    
            if(!success)
                return BadRequest(message);
        
            return Ok(new {message});
        }
    }
}
