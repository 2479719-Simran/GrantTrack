using System;
using GrantTrack.Dto;

namespace GrantTrack.Service.UserServices;

public interface IUserService
{
    public Task<UserUpdateResponseDto> UpdateUser(int id , UserUpdateRequestDto userUpdateRequestDto);
}
