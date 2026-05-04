namespace GrantTrack.Dto.DocumentDtos;

public class UploadDocumentResponseDto
{
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileURI { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime UploadedAt { get; set; }
}