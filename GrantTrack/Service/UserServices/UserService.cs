using System;
using GrantTrack.Dto;
using GrantTrack.Repository.UserRepositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GrantTrack.Service.UserServices;

public class UserService : IUserService
{   
    private readonly IUserRepository _userRepository; 

    public UserService(IUserRepository _userRepository)
    {
        this._userRepository = _userRepository; 
    }
    public async Task<UserUpdateResponseDto> UpdateUser(int id, UserUpdateRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new ArgumentException("Required fields are missing.");
        }
        var response = await _userRepository.UpdateUser(id ,request); 
        
        return response; 
    }

    
}
