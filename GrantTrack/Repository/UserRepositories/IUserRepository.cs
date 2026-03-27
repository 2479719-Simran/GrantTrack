using GrantTrack.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrantTrack.Repository.UserRepositories;

public interface IUserRepository
{
    

    // For your Admin View part
    Task<IEnumerable<User>> GetAllUsersAsync();
}