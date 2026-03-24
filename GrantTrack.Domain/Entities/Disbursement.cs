using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GrantTrack.Domain.Entites;
[Table("Disbursement")]
public class Disbursement
{
    [Key]
    public int DisbursementId { get; set; }
    [ForeignKey("ApplicationIdNavigation")]
    public int ApplicationId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    [Column(TypeName = "date")]
    public DateTime ScheduledDate { get; set; }
    [Column(TypeName = "date")]
    public DateTime? ActualDate { get; set; }
    public bool Status { get; set; }
    public virtual Application ApplicationIdNavigation { get; set; }
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

