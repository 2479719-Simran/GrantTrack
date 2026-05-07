using System;

namespace GrantTrack.Dto.ApplicationDtos;

public class ValidationResponseDto
{
    public int ApplicationValidationId { get; set; }
    public int ApplicationId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CheckedDate { get; set; }

}
