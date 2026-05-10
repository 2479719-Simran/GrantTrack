using System;

namespace GrantTrack.Dto.LoginDtos;

public class LoginResponseDto
{
    /// Indicates whether the login was successful
    public bool Success;
    // Contains the JWT token if login is successful, otherwise null
    public string? AccessToken { get; set; }
    // Contains an error message if login fails, otherwise null
    public string? ErrorMessage { get; set; }
}
