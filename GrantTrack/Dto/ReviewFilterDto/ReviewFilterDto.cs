using System;

namespace GrantTrack.Dto;

public class ReviewFilterDto
{
    public int ReviewerId { get; set; }
    public int PageNumber { get; set; } = 1; // Default page 1
    public int PageSize { get; set; } = 10; // Default 10 items

}
