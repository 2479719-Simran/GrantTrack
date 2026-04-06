using System;
using System.ComponentModel.DataAnnotations;
namespace GrantTrack.Dto.User;

public class RegisterUserDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    [Required]
    [RegularExpression(@"^\d{10}$",
    ErrorMessage = "Mobile number must be exactly 10 digits.")]
    public string Phone { get; set; } = null!;


}