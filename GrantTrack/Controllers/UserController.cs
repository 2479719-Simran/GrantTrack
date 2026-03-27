using System.Net;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.LoginDtos;
using GrantTrack.Service.UserServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly GrantTrackDbContext _context;
        private readonly IConfiguration _config;
        /// <summary>
        ///purpose: to handle user related operations such as login and registration. 
        /// </summary>
        /// <param name="userService">The user service instance</param>
        /// <param name="context">The database context</param>
        /// <param name="config">The configuration instance</param>
        public UserController(IUserService userService, GrantTrackDbContext context, IConfiguration config)
        {
            _userService = userService;
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
                return Unauthorized(new { error = loginResponse.ErrorMessage });
            }
            // If login is successful, return 200 OK with the JWT token    
            return Ok(loginResponse.AccessToken);
        }
    }
}
