using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.LoginDtos;
using GrantTrack.Dto.User;
using GrantTrack.Dto.UserDtos;
using GrantTrack.Dto.UserDTOs;
namespace GrantTrack.Service.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto, GrantTrackDbContext _context, IConfiguration _config);
        Task RegisterUserAsync(RegisterUserDto dto);
        Task<UpdateUserResponseDto> UpdateUser(int id, UpdateUserRequestDto request);
        // Matches your UserController's call
        Task<IEnumerable<ViewUserDto>> GetAllUsersAsync(GrantTrackDbContext context);
    }
}

