using GrantTrack.Domain.Entities;
using GrantTrack.Dto;
using GrantTrack.Dto.UserDtos;
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

        public async Task<bool> ActiveUserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && u.Status == true);
        }

        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UpdateUserResponseDto?> UpdateUser(int id, UpdateUserRequestDto request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }
            user.Name = request.Name;
            user.Phone = request.Phone;
            // Finding User role
            if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            {
                throw new ArgumentException($"Invalid role '{request.Role}'");
            }
            user.Role = role;
            user.Status = request.Status;
            user.Email = request.Email;
            await _context.SaveChangesAsync();
            return new UpdateUserResponseDto
            {
                Name = user.Name,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                Status = request.Status,
                Email = user.Email
            };
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower().Trim());
        }
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves all users for your Admin View part.
        /// </summary>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }
    }
}
