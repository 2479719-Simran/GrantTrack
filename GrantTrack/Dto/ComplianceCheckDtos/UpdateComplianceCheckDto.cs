using System;



namespace GrantTrack.Dto.ComplianceCheckDtos;

public class UpdateComplianceCheckDto
{
    public string Result { get; set; } // Completed or Flagged
    public string Notes { get; set; }  
}