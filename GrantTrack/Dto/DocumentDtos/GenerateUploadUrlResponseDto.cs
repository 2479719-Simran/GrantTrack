namespace GrantTrack.Dto.DocumentDtos;
 public class GenerateUploadUrlResponseDto
{
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public int DocumentId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
 