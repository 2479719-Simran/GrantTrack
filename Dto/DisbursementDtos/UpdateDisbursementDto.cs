using System;
using System.ComponentModel.DataAnnotations;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Dto.DisbursementDtos;

public class UpdateDisbursementDto
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal? Amount { get; set; }

    public DateTime? ScheduledDate { get; set; }

    public DateTime? ActualDate { get; set; }

    public DisbursementStatus? Status { get; set; }
}