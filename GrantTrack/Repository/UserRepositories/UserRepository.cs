using System;
using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.UserRepositories;

public class UserRepository : IUserRepository
{
    private readonly GrantTrackDbContext _context;

    public UserRepository(GrantTrackDbContext context)
    {
        _context = context;
    }
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
