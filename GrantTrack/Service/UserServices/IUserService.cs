using GrantTrack.Domain.Entities;
using GrantTrack.Dto.LoginDtos;
using GrantTrack.Dto.UserDTOs;

namespace GrantTrack.Service.UserServices;

public interface IUserService
{
    // Matches your Login logic
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto, GrantTrackDbContext _context, IConfiguration _config);

    // Matches your UserController's call
    Task<IEnumerable<ViewUserDto>> GetAllUsersAsync(GrantTrackDbContext context);
}