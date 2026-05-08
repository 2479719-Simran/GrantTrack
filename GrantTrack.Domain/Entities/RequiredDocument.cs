using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Domain.Entities;

public class RequiredDocument
{
    [Key]
    public int DocumentId { get; set; }
    public int ProgramId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool Mandatory { get; set; }

    [ForeignKey("ProgramId")]
    public virtual GrantProgram? Program { get; set; }
}
