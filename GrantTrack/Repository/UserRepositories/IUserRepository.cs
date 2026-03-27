using System;
using GrantTrack.Domain.Entities;
namespace GrantTrack.Repository.Interface
{
    public interface IUserRepository
{
    Task<bool> UserExistsAsync(string email);
    Task AddUserAsync(User user);
}
}
