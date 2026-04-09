using System;

namespace GrantTrack.Dto.ApplicationDtos;

public class ApplicationResponseDto
{
     public int ApplicationId { get; set; }
    public int ProgramId { get; set; }
    public int ApplicantId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedDate { get; set; }

}
