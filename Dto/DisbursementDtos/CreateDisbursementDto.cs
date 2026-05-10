using System;
using System.ComponentModel.DataAnnotations;

namespace GrantTrack.Dto.DisbursementDtos;

public class CreateDisbursementDto
{
    [Required]
    public int ApplicationId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime ScheduledDate { get; set; }
}
