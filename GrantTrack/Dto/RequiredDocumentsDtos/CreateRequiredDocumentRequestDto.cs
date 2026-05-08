using System;

namespace GrantTrack.Dto.RequiredDocumentsDtos;

public class CreateRequiredDocumentRequestDto
{
    public int ProgramId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
}
