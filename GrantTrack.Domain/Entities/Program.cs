using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace GrantTrack.Domain.Entities;

public class Program
{
    [Key]    
    public int ProgramId { get; set; }
    [Required]
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Decimal Budget { get; set; }
    public bool Status { get; set; }
    public ICollection<EligibilityRule> EligibilityRules { get; set; } 
    public ICollection<RequiredDocument> RequiredDocuments { get; set; } 
    public List<Application> Applications { get; set; } = new List<Application>();
}
