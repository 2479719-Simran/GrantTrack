using System;
using GrantTrack.Dto.User;
namespace GrantTrack.Service.Interfaces
{
    public interface IUserService
    {
        Task RegisterUserAsync(RegisterUserDto dto);
    }
}

