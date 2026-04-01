using System;
using System.Text.RegularExpressions;
using GrantTrack.Dto.UserDtos;
using GrantTrack.Repository.Interface;
using GrantTrack.Utility;

namespace GrantTrack.Service.AuthServices;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto model)
    {
        try
        {
            // Validate password match
            if (model.NewPassword != model.ConfirmPassword)
                return (false,  Messages.PasswordMismatch);
            // Validate password strength
            if (!IsValidPassword(model.NewPassword))
                return (false, Messages.WeakPassword);
            

        var user = await _userRepository.GetUserByEmailAsync(model.Email);
        if (user == null)
            return (false, Messages.UserNotFound);

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.Password = hashedPassword; 

        await _userRepository.UpdateUserAsync(user);
        return (true, Messages.PasswordUpdated);
        }
         catch (Exception ex)
        {
            return (false, Messages.SomethingWentWrong);
        }
    }
    private bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
 
        // Min 8 chars, at least one uppercase, one lowercase, one digit, one special char
        var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";    
        return Regex.IsMatch(password, pattern);  
    }
}
