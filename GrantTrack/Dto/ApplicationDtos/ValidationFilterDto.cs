using System;

namespace GrantTrack.Dto.ApplicationDtos;

public class ValidationFilterDto
{
    public int? ApplicationId { get; set; }
    public string? Result { get; set; }   // "Passed" or "Failed"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

}
