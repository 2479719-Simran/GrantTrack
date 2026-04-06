using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GrantTrack.Domain.Entities;

public class EligibilityRule
{
    [Key]
    public int RuleId { get; set; }
    [ForeignKey("programId")]
    public int ProgramId { get; set; }
    public string RuleDescription { get; set; }
    [Column(TypeName = "VARCHAR(MAX)")]
    public string RuleExpression { get; set; }
    public virtual GrantProgram? programIDNavigation { get; set; }
}
