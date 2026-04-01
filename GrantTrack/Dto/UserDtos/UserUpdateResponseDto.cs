using System;

namespace GrantTrack.Dto;

public class UserUpdateResponseDto
{
   public string? Name { get; set; }
    public string? Email { get; set; }
    public int Phone { get; set; }
    public bool Status { get; set; }
    public string? Password{get; set;} 
}
