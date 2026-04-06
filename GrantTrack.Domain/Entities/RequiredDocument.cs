using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace GrantTrack.Domain.Entities;

[PrimaryKey("DocumentId")]
public class RequiredDocument
{
    [Key]
    public int DocumentId { get; set; }
    [ForeignKey("ProgramId")]
    public int ProgramId { get; set; }
    [Required]
    public string? Name { get; set; }
    public bool Mandatory { get; set; }
    public virtual Program? ProgramIDNavigation { get; set; }


}
