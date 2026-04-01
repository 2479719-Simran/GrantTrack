using System;
using System.Text.RegularExpressions;

namespace GrantTrack.Utility;

public class PhoneNumberValidator
{
    public static bool PhoneNumberValidation(string phone)
    {
        bool isValid = Regex.IsMatch(phone, @"^\d{10}$");
        return isValid;
    }
}