using System;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.UserRepositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);

}
