using System;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GrantTrack.Domain.Entities;

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
    public string? Decision { get; set; }
    public string Notes { get; set; }

    [Column(TypeName = "DATE")]
    public DateTime Date { get; set; }
    [ForeignKey("ApplicationIdNavigation")]
    public virtual Application? ApplicationIdNavigation { get; set; }
    [ForeignKey("ReviewerIdNavigation")]
    public virtual User? ReviewerIdNavigation { get; set; }
}