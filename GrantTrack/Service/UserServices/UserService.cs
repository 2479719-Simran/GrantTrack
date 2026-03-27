using System;
using GrantTrack.Controllers;
using GrantTrack.Dto.LoginDtos;
using GrantTrack.Domain.Entities;
using GrantTrack.Repository.UserRepositories;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Azure.Core;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.UserServices;

public class UserService : IUserService
{
    private async Task<string> GenerateJwtTokenServiceAsync(GrantTrack.Domain.Entities.User user, IConfiguration config)
    {
        var secretKey = config["JwtSettings:SecretKey"];
        if(string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured.");
        }
        var issuer=config["JwtSettings:Issueer"];
        var audience=config["JwtSettings:Audience"];
        var expiryMinutes=int.TryParse(config["JwtSettings:Expiry"],out var minutes)?minutes:60;

        var SecurityKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var SecurityAlgorithm=new SigningCredentials(SecurityKey,SecurityAlgorithms.HmacSha256);
        var claims=new []
        {
            new Claim(JwtRegisteredClaimNames.Sub,user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email,user.Email),
            new Claim(ClaimTypes.Role,user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
        };
        var Token=new JwtSecurityToken
        (
            issuer:issuer,
            audience:audience,
            claims:claims,
            expires:DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials:SecurityAlgorithm
        );
        return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(Token));
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto, GrantTrackDbContext _context, IConfiguration _config)
    {
        if(string.IsNullOrWhiteSpace(loginRequestDto.Email) || string.IsNullOrWhiteSpace(loginRequestDto.Password))
        {
            return new LoginResponseDto
            {
                Success=false ,
                ErrorMessage="Email or Password cannot be mmpty"
            };
        }

        var user=await _context.Users.FirstOrDefaultAsync(u => u.Email== loginRequestDto.Email);
        if(user == null)
        {
            return new LoginResponseDto
            {
                Success=false,
                ErrorMessage="User not Found"
            };
        }

        var isPassword=BCrypt.Net.BCrypt.Verify(loginRequestDto.Password,user.Password);
        if(!isPassword)
        {
            return new LoginResponseDto
            {
                Success=false,
                ErrorMessage="Invalid Password"
            };
        }

        var token=await GenerateJwtTokenServiceAsync(user,_config);
        return new LoginResponseDto
        {
            Success=true,
            AccessToken=token
        };
    }
}