namespace GrantTrack.Dto.RequiredDocumentsDtos;

// ----- GetRequiredDocumentResponseDto.cs -----


public class GetRequiredDocumentResponseDto
{
    public int DocumentId { get; set; }
    public int ProgramId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
}
