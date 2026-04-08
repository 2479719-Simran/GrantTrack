using System;

namespace GrantTrack.Dto.ProgramDtos;

public class FilterProgramsDto
{
    public string? Status { get; set; } 
    public DateTime? StartDate { get; set; } = null;
    public DateTime? EndDate { get; set; } =null; 
}
