using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GrantTrack.Domain.Entities;

[Table("Review")]
public class Review
{
    [Key]
    public int ReviewId { get; set; }
    [Required]
    public int ApplicationId { get; set; }
    [Required]
    public int ReviewerId { get; set; }
    
    public int Score { get; set; }

    [Column(TypeName = "VARCHAR(MAX)")]
    public string Comments { get; set; }

    public DateTime Date { get; set; }

    // Navigation Properties
    [ForeignKey("ApplicationId")]
    public virtual Application ?ApplicationIDNavigation { get; set; }

    [ForeignKey("ReviewerId")]
    public virtual User ?ReviewerIDNavigation { get; set; }
}
