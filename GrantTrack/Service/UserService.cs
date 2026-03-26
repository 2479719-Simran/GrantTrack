using GrantTrack.Domain.Entities;
using GrantTrack.Dto.User;
using GrantTrack.Repository.Interface;
using GrantTrack.Service.Interfaces;
using GrantTrack.Utility;

namespace GrantTrack.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task RegisterUserAsync(RegisterUserDto dto)
        {
            // Password validation
            if (!PasswordValidator.IsValid(dto.Password))
            {
                throw new ArgumentException(
                    "Password must be at least 8 characters and contain one uppercase letter and one number");
            }

            // Email uniqueness
            if (await _userRepository.UserExistsAsync(dto.Email))
            {
                throw new InvalidOperationException("User already exists");
            }

            // BCrypt handles salt internally, but since you store it separately:
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password, salt);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                RoleId = 1,               //  Applicant role (configure in DB)
                Status = true,            //  Active user
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUserAsync(user);
        }
    }
}
