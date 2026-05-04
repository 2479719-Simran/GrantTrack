using System;

namespace GrantTrack.Dto.DisbursementDtos;

public class PagedResponseDto<T>
{
    public int Page           { get; set; }
    public int PageSize       { get; set; }
    public int TotalRecords   { get; set; }
    public int TotalPages     { get; set; }
    public IEnumerable<T> Data { get; set; } = new List<T>();
}