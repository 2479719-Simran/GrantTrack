using System;

namespace GrantTrack.Utility;

public static class Messages
{
    public const string InvalidRequest = "Invalid request.";
    public const string SomethingWentWrong = "Something went wrong. Please try again.";
    public const string Success = "Operation completed successfully.";
    public const string UserNotFound = "No account found with that email address.";
    public const string PasswordMismatch = "Passwords do not match.";
    public const string WeakPassword = "Password must be at least 8 characters long and include at least one uppercase letter, one lowercase letter, one digit, and one special character.";
    public const string PasswordUpdated = "Password updated successfully.";
    public const string EmailRequired = "Email is required.";
    public const string EmailInvalid = "Invalid email format.";
    public const string NewPasswordRequired = "New password is required.";
    public const string ConfirmPasswordRequired = "Confirm password is required.";
    public const string UserDeactivated = "User has been successfully deactivated.";
    public const string UserNotFoundById = "No account found with the provided ID.";
    public const string UserAlreadyInactive = "This user account is already inactive.";
    public const string ApplicationNotFound = "Application not found.";
    public const string ApplicationNotInDraft = "Only Draft applications can be submitted.";
    public const string Forbidden = "You do not have permission to perform this action.";
}

