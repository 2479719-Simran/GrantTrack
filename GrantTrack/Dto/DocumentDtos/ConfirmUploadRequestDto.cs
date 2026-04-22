using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GrantTrack.Dto.DocumentDtos;

public class ConfirmUploadRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ApplicationId must be a positive integer.")]
    public int ApplicationId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DocumentId must be a positive integer.")]
    public int DocumentId { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!;
}