using System;

namespace GrantTrack.Dto.UserDTOs;

public class UpdateUserStatusDto
{
    public int UserID { get; set; }
    public bool Status { get; set; }
}
