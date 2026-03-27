using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrantTrack.Repository;
using GrantTrack.Dto.UserDTOs; 
using GrantTrack.Domain.Entities;

namespace GrantTrack.Service;
/// <summary>
/// Service for handling user-related business operations.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    /// <summary>
    /// Fetches all users and maps them to ViewUserDto for administrative viewing.
    /// </summary>
    /// <returns>A collection of mapped user DTOs.</returns>
    public async Task<IEnumerable<ViewUserDto>> GetAllUsersForAdminAsync()
    {
        // Retrieve all records from the repository
        var users = await _userRepository.GetAllUsersAsync();
        // Convert entities to DTOs while avoiding type mismatch errors
        return users.Select(u => new ViewUserDto
        {
            UserId = u.UserId,
            Name = u.Name,
            Email = u.Email,
            Status = u.Status,
            RoleId = u.RoleId
        }).ToList();
    }
}