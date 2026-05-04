using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GrantTrack.Dto.DocumentDtos;

public class UploadDocumentRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ApplicationId must be a positive integer.")]
    public int ApplicationId { get; set; }

    [Required]
    public string DocType { get; set; } = string.Empty;

    [Required]
    public IFormFile File { get; set; } = null!;
}