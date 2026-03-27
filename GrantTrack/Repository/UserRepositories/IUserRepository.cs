using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.UserDtos;
namespace GrantTrack.Repository.Interface
{
    public interface IUserRepository
{
    Task<bool> ActiveUserExistsAsync(string email);
    Task AddUserAsync(User user);
    Task<UpdateUserResponseDto?> UpdateUser(int id , UpdateUserRequestDto request); 
}
}
