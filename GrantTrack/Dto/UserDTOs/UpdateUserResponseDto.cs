using System;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Dto.UserDtos;

public class UpdateUserResponseDto
{
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;  
}
