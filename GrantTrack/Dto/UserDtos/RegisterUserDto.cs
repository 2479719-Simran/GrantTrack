using System;
using System.ComponentModel.DataAnnotations;
using GrantTrack.Domain.Entities;
namespace GrantTrack.Dto.User;

public class RegisterUserDto
{
    public string Name { get; set; } = null!;

    [Required]
    [RegularExpression(
    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
    ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    [Required]
    [RegularExpression(@"^\d{10}$",
    ErrorMessage = "Mobile number must be exactly 10 digits.")]
    public string Phone { get; set; } = null!;
    public UserRole? Role { get; set; }

}