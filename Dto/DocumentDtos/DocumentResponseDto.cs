namespace GrantTrack.Dto.DocumentDtos;

/// <summary>
/// Shape returned by the documents endpoints. We expose the original file
/// name (parsed from FileURI) plus a guarded download URL — never the
/// physical disk path.
/// </summary>
public class DocumentResponseDto
{
    public int DocumentId { get; set; }
    public int ApplicationId { get; set; }
    public string DocType { get; set; } = string.Empty;
    /// <summary>The original file name as uploaded by the user.</summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>Relative URL the FE should hit to download. Auth required.</summary>
    public string DownloadUrl { get; set; } = string.Empty;
}
