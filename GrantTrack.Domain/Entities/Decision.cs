using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore;


namespace GrantTrack.ADM.Domain.Models
{
      
    [Table("Decision")]
    public class Decision
    {
        [Key]
        public int DecisionId { get; set; }
        [ForeignKey("ApplicationIdNavigation")]
        [Required]
        public int ApplicationId { get; set; } 
        public virtual Application? ApplicationIdNavigation { get; set; }
        [ForeignKey("ApproverIdNavigation")]
        [Required]
        public int ApproverID { get; set; }
        public virtual User? ApproverIdNavigation { get; set; }
        [Required]
        public bool DecisionValue { get; set; } 
        [MaxLength(1000)]
        public string? Notes { get; set; }
        public DateTime Date { get; set; } 
    }
}