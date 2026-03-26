using System;

namespace GrantTrack.Dto.User;

public class RegisterUserDto
{
       public int UserId { get; set; }
       public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int Phone { get; set; }
}
