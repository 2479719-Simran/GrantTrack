using System;
using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.UserRepositories;

public class UserRepository : IUserRepository
{
    private readonly GrantTrackDbContext _context;
    public UserRepository(GrantTrackDbContext context)
    {
        _context=context;
    }

}
