using System;

namespace GrantTrack.Dto.ProgramDtos;

public class UpdateProgramRequestDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Decimal Budget { get; set; }
    public bool Status { get; set; }
}
