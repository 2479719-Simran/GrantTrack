using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrantTrack.Domain.Entities;

// 1. Define the Enum for the statuses
public enum DecisionStatus
{

    Approved ,
    Rejected 
    
}

// 2. Define the Decision Entity
public class Decision
{
    [Key]
    public int DecisionId { get; set; } 

    [Required]
    [EnumDataType(typeof(DecisionStatus))]
    public DecisionStatus DecisionValue { get; set; } 

    [Required]
    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty; 

    [Required]
    public DateTime Date { get; set; } = DateTime.Now; // Local time as requested

    [Required]
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [Required]
    public int ApplicationId { get; set; } 

    [ForeignKey("ApplicationId")] 
    public Application? Application { get; set; }
}