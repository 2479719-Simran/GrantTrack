using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using GrantTrack.Service;
using GrantTrack.Dto.UserDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace GrantTrack.Controllers;

/// <summary>
/// Provides API endpoints for managing users within the GrantTrack system.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userService">The service used for user business logic.</param>
    /// <param name="logger">The logger instance for error tracking.</param>
    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of all active and inactive users for administrative review.
    /// </summary>
    /// <remarks>
    /// This endpoint is restricted to users with the 'Admin' role.
    /// URL: GET /api/v1/user/GetAll
    /// </remarks>
    /// <returns>A collection of <see cref="ViewUserDto"/> objects.</returns>
    [HttpGet("GetAll")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ViewUserDto>>> GetAll()
    {
        try
        { 
            // Request the list of users from the service layer
            var users = await _userService.GetAllUsersForAdminAsync();

            // Return a 200 OK status with the user data
            return Ok(users);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Log security-related failures for audit purposes
            _logger.LogWarning(ex, "Unauthorized access attempt to GetAll users.");
            return Forbid("Access denied: Insufficient permissions.");
        }
        catch (Exception ex)
        {
            // Log the detailed exception internally for debugging
            _logger.LogError(ex, "An unexpected error occurred while retrieving the user list.");

            // Return a generic 500 error to avoid exposing sensitive system details
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An internal server error occurred. Please contact system support.");
        }
    }
}