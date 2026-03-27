using System;
using GrantTrack.Domain.Entities;
namespace GrantTrack.Repository.Interface
{
    public interface IUserRepository
{
    Task<bool> ActiveUserExistsAsync(string email);
    Task AddUserAsync(User user);
}
}
