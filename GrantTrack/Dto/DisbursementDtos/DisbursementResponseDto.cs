using System;

namespace GrantTrack.Dto.DisbursementDtos;

public class DisbursementResponseDto
{
    public int DisbursementId     { get; set; }
    public int ApplicationId      { get; set; }
    public decimal Amount         { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? ActualDate   { get; set; }
    public string Status          { get; set; } = string.Empty; // returns enum name as string e.g. "Scheduled" 
}
