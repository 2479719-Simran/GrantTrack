using System;
using System.ComponentModel.DataAnnotations;

namespace GrantTrack.Dto.ReviewDtos;

public class BulkAssignmentDto
{
    public List<AssignmentItemDto> Assignments { get; set; } = new();
}
public class AssignmentItemDto
{
    public int ApplicationId { get; set; }
    public int ReviewerId { get; set; }
    public int Score {get; set;}=0;
    public string Comments { get; set; } 
}
