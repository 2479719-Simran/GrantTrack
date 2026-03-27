using System;
using GrantTrack.Controllers;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.LoginDtos;

namespace GrantTrack.Service.UserServices;

public interface IUserService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto, GrantTrackDbContext _context, IConfiguration _config);
}
