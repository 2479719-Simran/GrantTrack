using System;
namespace GrantTrack.Utility;
public class PasswordHasher
{
    public static string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }


