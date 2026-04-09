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
    [RegularExpression(@"^([1-9]|10)$", ErrorMessage = "Score must be between 1 and 10")]
    public int Score {get; set;}
    public string Comments { get; set; } 
}
