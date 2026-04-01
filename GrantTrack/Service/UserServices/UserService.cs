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
            // Validate password rules
            if (!PasswordValidator.IsValid(dto.Password))
            {
                throw new ArgumentException(
                    "Password must be at least 8 characters and contain one uppercase letter and one number");
            }

            // Check duplicate email
            if (await _userRepository.ActiveUserExistsAsync(dto.Email))
            {
                throw new InvalidOperationException("Active User with this email already exists");
            }
            // Hash password (BCrypt)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var roleToAssign = UserRole.Applicant; // Default

            if (dto.Role.HasValue)
            {
                // BLOCK ADMIN EXPLICITLY
                if (dto.Role.Value == UserRole.Admin)
                {
                    throw new ArgumentException("Admin role cannot be assigned during registration");
                }

                //  Allow only safe roles
                roleToAssign = dto.Role.Value;
            }
            // Create User entity 
            var user = new User
            {
                Name = dto.Name,
                Role = roleToAssign, // Convert enum to string
                Email = dto.Email,
                Phone = dto.Phone,
                Status = true,
                // REQUIRED because User.Password is [Required]
                Password = hashedPassword,

                CreatedAt = DateTime.UtcNow
            };
            // 5. Save user
            await _userRepository.AddUserAsync(user);
        }
    }
}