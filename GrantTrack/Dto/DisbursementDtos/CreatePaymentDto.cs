using System;
using System.ComponentModel.DataAnnotations;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Dto.DisbursementDtos;

public class CreatePaymentDto
{
    [Required]
    public int DisbursementId { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335.99", ErrorMessage = "Amount must be greater than zero")]
    [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Amount allows max 2 decimal places")]

    public decimal Amount { get; set; } =  0.00m;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public PaymentMethod Method { get; set; }
}