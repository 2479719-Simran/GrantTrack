using System;

namespace GrantTrack.Dto.DisbursementDtos;

public class PaymentResponseDto
{
    public int PaymentId      { get; set; }
    public int DisbursementId { get; set; }
    public decimal Amount     { get; set; }
    public DateTime Date      { get; set; }
    public string Method      { get; set; } = string.Empty;
    public string Status      { get; set; } = string.Empty;
}