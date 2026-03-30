using System;
using GrantTrack.Dto.UserDtos;

namespace GrantTrack.Service.AuthServices;

public interface IAuthService
{
    Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto model);

}
