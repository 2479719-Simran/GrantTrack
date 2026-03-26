using System;
using System.Text.RegularExpressions;
namespace GrantTrack.Utility;
public class PasswordValidator
{
    public static bool IsValid(string password)
        {
            if (password.Length < 8) return false;
            if (!Regex.IsMatch(password, "[A-Z]")) return false;
            if (!Regex.IsMatch(password, "[0-9]")) return false;

            return true;
        }
    }



