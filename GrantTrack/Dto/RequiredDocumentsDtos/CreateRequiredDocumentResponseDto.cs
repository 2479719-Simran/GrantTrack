namespace GrantTrack.Dto.RequiredDocumentsDtos;

// ----- CreateRequiredDocumentResponseDto.cs -----


public class CreateRequiredDocumentResponseDto
{
    public int DocumentId { get; set; }
    public int ProgramId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
}
