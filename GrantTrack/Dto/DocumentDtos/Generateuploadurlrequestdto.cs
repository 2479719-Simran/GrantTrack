using System.ComponentModel.DataAnnotations;

namespace GrantTrack.Dto.DocumentDtos;

public class GenerateUploadUrlRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ApplicationId must be a positive integer.")]
    public int ApplicationId { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty;
    [Required]
    [Range(1, 10485760, ErrorMessage = "File size must be less than 10 MB.")]
    public long FileSize { get; set; }
    [Required]
    public string DocType { get; set; } = string.Empty;
}