using System;

namespace GrantTrack.Dto.ApplicationDtos;

public class ValidationMessageDto
{
    public string RuleName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
