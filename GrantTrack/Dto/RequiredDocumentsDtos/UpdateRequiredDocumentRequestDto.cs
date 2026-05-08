namespace GrantTrack.Dto.RequiredDocumentsDtos;

// ----- UpdateRequiredDocumentRequestDto.cs -----


public class UpdateRequiredDocumentRequestDto
{
    public int ProgramId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
}
