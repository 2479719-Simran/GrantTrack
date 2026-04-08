using System;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GrantTrack.Domain.Entities;
public enum ReviewDecision
{
    Pending,
    Approved,
    Rejected
}
[Table("Recommendation")]
public class Recommendation
{
    [Key]
    public int RecommendationId { get; set; }
    [Required]
    public int ApplicationId { get; set; }
    [Required]
    public int ReviewerId { get; set; }
    [Column(TypeName = "VARCHAR(MAX)")]
    [EnumDataType(typeof(ReviewDecision), ErrorMessage = "Invalid decision value. Please select Pending, Approved, or Rejected.")]
    public ReviewDecision Decision { get; set; }
    public string Notes { get; set; }

    [Column(TypeName = "DATE")]
    public DateTime Date { get; set; }
    [ForeignKey("ApplicationId")]
    public virtual Application? ApplicationIdNavigation { get; set; }
    [ForeignKey("ReviewerId")]
    public virtual User? ReviewerIdNavigation { get; set; }
}