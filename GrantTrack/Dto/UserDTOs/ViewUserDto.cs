using System;

namespace GrantTrack.Dto.UserDTOs;

public class ViewUserDto
{
   public int UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool Status { get; set; }
    public string? RoleName { get; set; }
}
