using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GrantTrack.Domain.Entities;
public enum DisbursementStatus
{
    Pending,
    Scheduled,
    Paid,
    PartiallyPaid,
    Cancelled
}
public class Disbursement
{
        [Key]      
        public int DisbursementId { get; set; }

        // Foreign Key to the Application table
        public int ApplicationId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "date")]
        public DateTime ScheduledDate { get; set; }
        // Made nullable (DateTime?) because the ActualDate might be null until it is paid
        [Column(TypeName = "date")]
        public DateTime? ActualDate { get; set; }
        [MaxLength(50)]
        public DisbursementStatus Status { get; set; } // e.g., Scheduled/Paid/Partially Paid/Cancelled 
        // I think we need to keep status as bool 

        
         [ForeignKey("ApplicationId")]
         public virtual Application Application { get; set; }

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>(); 

        
}
