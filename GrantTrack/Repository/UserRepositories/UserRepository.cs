using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrantTrack.Repository.UserRepositories;

public class UserRepository : IUserRepository
{
    private readonly GrantTrackDbContext _context;

    public UserRepository(GrantTrackDbContext context)
    {
        _context = context;
    }

 
    /// <summary>
    /// Retrieves all users for your Admin View part.
    /// </summary>
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.AsNoTracking().ToListAsync();
    }
}