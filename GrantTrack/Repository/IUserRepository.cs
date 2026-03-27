using System.Collections.Generic;
using System.Threading.Tasks;
using GrantTrack.Domain.Entities; 

namespace GrantTrack.Repository;
public interface IUserRepository
{
  
    Task<IEnumerable<User>> GetAllUsersAsync();
}