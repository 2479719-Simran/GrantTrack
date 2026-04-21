using System;
namespace GrantTrack.Dto.ComplianceCheckDtos;

public class ComplianceCheckDto
{
    public int ApplicationId { get; set; }
    public string Type { get; set; } // Financial or Operational
    public string Notes { get; set; } // Initial notes
}