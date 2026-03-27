using Microsoft.EntityFrameworkCore;
using GrantTrack.Domain.Entities; 

namespace GrantTrack.Repository;

public class UserRepository : IUserRepository
{
    private readonly GrantTrackDbContext _context;

    // Inject the Database Context
    public UserRepository(GrantTrackDbContext context)
    {
        _context = context;
    }

   
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.RoleIDNavigation) 
            .AsNoTracking()  //fetching data as read-only,for better performance               
            .ToListAsync();
    }
}