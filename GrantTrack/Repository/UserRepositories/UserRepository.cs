using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto;

namespace GrantTrack.Repository.UserRepositories;

public class UserRepository : IUserRepository
{   private readonly GrantTrackDbContext _context;

    public UserRepository(GrantTrackDbContext _context)
    {
        this._context = _context; 
    }
    public async Task<UserUpdateResponseDto> UpdateUser(int id, UserUpdateRequestDto userUpdateRequestDto)
    {
        var user = await _context.Users.FindAsync(id); 
        if(user == null)
        {
            return null; 
        } 

        user.Name = userUpdateRequestDto.Name; 
        user.Email = userUpdateRequestDto.Email; 
        user.Phone = userUpdateRequestDto.Phone; 
        user.Status = userUpdateRequestDto.Status; 
        user.Password = userUpdateRequestDto.Password; 

        await _context.SaveChangesAsync(); 

        return new UserUpdateResponseDto
        {
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Password = user.Password,
            Status = user.Status
        }; 


        
    }

    
}
