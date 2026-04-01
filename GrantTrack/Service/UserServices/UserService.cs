using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.LoginDtos;
using GrantTrack.Dto.User;
using GrantTrack.Dto.UserDtos;
using GrantTrack.Repository.Interface;
using GrantTrack.Service.Interfaces;
using GrantTrack.Utility;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using GrantTrack.Dto.UserDTOs;

namespace GrantTrack.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto, GrantTrackDbContext _context, IConfiguration _config)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(loginRequestDto.Email) || string.IsNullOrWhiteSpace(loginRequestDto.Password))
            {
                return new LoginResponseDto
                {
                    Success = false,
                    ErrorMessage = "Email or Password cannot be empty"
                };
            }
            // Check if user exists and is active
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequestDto.Email);
            if (user == null || !user.Status)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    ErrorMessage = "No active account found with the provided email address"
                };
            }
            // Verify password
            var isPassword = BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, user.Password);
            if (!isPassword)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    ErrorMessage = "Invalid Password"
                };
            }
            // Generate JWT token
            var token = await GenerateJwtTokenServiceAsync(user, _config);
            return new LoginResponseDto
            {
                Success = true,
                AccessToken = token
            };
        }
        private async Task<string> GenerateJwtTokenServiceAsync(GrantTrack.Domain.Entities.User user, IConfiguration config)
        {
            // Retrieve JWT settings from configuration
            var secretKey = config["JwtSettings:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }
            var issuer = config["JwtSettings:Issueer"];
            var audience = config["JwtSettings:Audience"];
            var expiryMinutes = int.TryParse(config["JwtSettings:Expiry"], out var minutes) ? minutes : 60;
            // Create signing credentials
            var SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var SecurityAlgorithm = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
            // Define claims
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub,user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email,user.Email),
            new Claim(ClaimTypes.Role,user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
        };
            var Token = new JwtSecurityToken
            (
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: SecurityAlgorithm
            );
            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(Token));
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

            // Create User entity 
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = UserRole.Applicant,
                Status = true,
                // REQUIRED because User.Password is [Required]
                Password = hashedPassword,

                CreatedAt = DateTime.UtcNow
            };
            // 5. Save user
            await _userRepository.AddUserAsync(user);
        }

        public async Task<UpdateUserResponseDto> UpdateUser(int id, UpdateUserRequestDto request)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.Phone, @"^[6-9]\d{9}$"))
            {
                throw new Exception("Password should start wit 6,7,8,9 and has 10 digits only");
            }

            var res = await _userRepository.UpdateUser(id, request);

            return res;
        }

        /// <summary>
        /// Fetches all users and maps them to ViewUserDto. 
        /// Matches the call from UserController.
        /// </summary>
        public async Task<IEnumerable<ViewUserDto>> GetAllUsersAsync(GrantTrackDbContext _context)
        {
            return await _context.Users
                .Select(user => new ViewUserDto
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    Status = user.Status
                })
                .ToListAsync();
        }
    }
}