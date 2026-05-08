namespace GrantTrack.Dto.RequiredDocumentsDtos;

// ----- UpdateRequiredDocumentResponseDto.cs -----


public class UpdateRequiredDocumentResponseDto
{
    public int DocumentId { get; set; }
    public int ProgramId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
}
