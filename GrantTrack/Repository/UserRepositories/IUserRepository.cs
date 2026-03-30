using System;
using GrantTrack.Dto;

namespace GrantTrack.Repository.UserRepositories;

public interface IUserRepository
{
    public Task<UserUpdateResponseDto> UpdateUser(int id ,UserUpdateRequestDto userUpdateRequestDto); 
}
