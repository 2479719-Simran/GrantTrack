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
    public const string DisbursementCreated                 = "Disbursement tranche created successfully.";
    public const string DisbursementUpdated                 = "Disbursement tranche updated successfully.";
    public const string DisbursementNotFound                = "Disbursement not found.";
    public const string DisbursementScheduledDateInPast     = "Scheduled date cannot be in the past.";
    public const string DisbursementCannotBeModified        = "Disbursement cannot be modified once it is Paid or Cancelled.";
    public const string DisbursementInvalidStatusTransition = "Cannot transition status from '{0}' to '{1}'.";
    public const string ApplicationNotFound     = "Application not found.";
    public const string Forbidden               = "You are not authorized to perform this action.";
    public const string ApplicationNotInDraft   = "Application is not in Draft status.";

    // Controller response messages
    public const string ApplicationSubmitted    = "Application submitted successfully.";
    public const string UnexpectedError         = "An unexpected error occurred. Please try again later.";
    public const string UserNotAuthenticated    = "User not authenticated.";
    public const string ProgramNotFound         = "Program not found.";

    // ← newly added for application business rules
    public const string ProgramNotActive        = "Applications can only be submitted to active programs.";
    public const string DuplicateApplication    = "You have already created the draft for this program.";
}