using System.ComponentModel.DataAnnotations;

namespace GrantTrack.Dto.DocumentDtos;

public class GenerateUploadUrlRequestDto
{
    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public string DocType { get; set; } = string.Empty;
}