using System;
using System.ComponentModel.DataAnnotations;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Dto.DisbursementDtos;

public class CreatePaymentDto
{
    [Required]
    public int DisbursementId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public PaymentMethod Method { get; set; }
}