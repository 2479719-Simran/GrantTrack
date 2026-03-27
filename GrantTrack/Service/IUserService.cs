using System.Collections.Generic;
using System.Threading.Tasks;
using GrantTrack.Dto;
using GrantTrack.Dto.UserDTOs;

namespace GrantTrack.Service;


public interface IUserService
{
    
    Task<IEnumerable<ViewUserDto>> GetAllUsersForAdminAsync();
}