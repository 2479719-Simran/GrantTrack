using System;

namespace GrantTrack.Dto.LoginDtos;

public class LoginResponseDto
{
    public bool Success;
    public string? AccessToken { get; set; }
    public string? ErrorMessage { get; set; }
}
