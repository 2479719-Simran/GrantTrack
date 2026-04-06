using System;
using System.Text.RegularExpressions;

namespace GrantTrack.Utility;

public class UpdateEmailValidator
{
    public static bool EmailValidation(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }
}
