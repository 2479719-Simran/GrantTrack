using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GrantTrack.Domain.Entities;

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
}

public enum PaymentMethod
{
    BankTransfer,
    Cheque,
    Cash,
    OnlineTransfer
}

[Table("Payment")]
public class Payment
{
        [Key]
        public int PaymentId { get; set; }
        [ForeignKey("DisbursementID")]
        public int DisbursementId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        [MaxLength(50)]
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public virtual Disbursement? DisbursementIDNavigation { get; set; }
}