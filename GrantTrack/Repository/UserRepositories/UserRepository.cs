using GrantTrack.Domain.Entities;
using GrantTrack.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly GrantTrackDbContext _context;

        public UserRepository(GrantTrackDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}