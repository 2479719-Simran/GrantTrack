using System;
using GrantTrack.Dto.User;
using GrantTrack.Dto.UserDtos;
namespace GrantTrack.Service.Interfaces
{
    public interface IUserService
    {
        Task RegisterUserAsync(RegisterUserDto dto);
        Task<UpdateUserResponseDto> UpdateUser(int id , UpdateUserRequestDto request);
    }
}

