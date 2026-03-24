using System;

namespace GrantTrack.Domain.Entities;

public class ApplicationValidation
{
    public int ApplicationValidationId { get; set; } 
    public Application Application { get; set; }     
    public int ApplicationId { get; set; }
    public string RuleName { get; set; } 
    public string Result { get; set; } 
    public string Message { get; set; }
    public DateTime CheckedDate { get; set; } 
}
