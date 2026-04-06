using System;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Utility;

public class RoleValidator
{
    public static bool RoleValidation(string role)
    {
        bool isValid = Enum.IsDefined(typeof(UserRole), role);
        return isValid;
    }
}
