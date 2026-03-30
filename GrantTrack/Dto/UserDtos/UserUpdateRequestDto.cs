using System;

namespace GrantTrack.Dto;

using System;



public class UserUpdateRequestDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Phone { get; set; }
    public bool Status { get; set; }
    public string Password{get; set;} 
}

