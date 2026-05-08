namespace GrantTrack.Dto.RequiredDocumentsDtos;

// ----- DeleteRequiredDocumentResponseDto.cs -----


public class DeleteRequiredDocumentResponseDto
{
    public int DocumentId { get; set; }
    public int ProgramId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
}
