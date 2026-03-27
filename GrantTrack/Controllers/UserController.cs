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
        public UserController(IUserService userService,GrantTrackDbContext context,IConfiguration config)
        {
            _userService=userService;
            _context=context;
            _config=config;
        }
    
    [HttpPost("login")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            var loginResponse=await _userService.LoginAsync(loginRequest,_context,_config);
            if(!loginResponse.Success)
            {
                return Unauthorized(new{error=loginResponse.ErrorMessage});
            }
            return Ok(loginResponse.AccessToken);
        }
    }
}
