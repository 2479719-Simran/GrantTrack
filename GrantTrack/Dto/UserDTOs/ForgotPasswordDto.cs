using System;
using System.ComponentModel.DataAnnotations;

namespace GrantTrack.Dto.UserDtos;

public class ForgotPasswordDto
{
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

       [Required(ErrorMessage = "New password is required.")]
        // [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase and a number.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

}
