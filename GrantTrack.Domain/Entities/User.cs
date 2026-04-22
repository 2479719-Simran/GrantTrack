using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Domain.Entities;

public enum UserRole
{
    [EnumMember(Value = "Admin")]
    Admin,
    [EnumMember(Value = "Applicant")]
    Applicant,
    [EnumMember(Value = "Reviewer")]
    Reviewer,
    [EnumMember(Value = "Approver")]
    Approver,
    [EnumMember(Value = "FinanceOfficer")]
    FinanceOfficer,
    [EnumMember(Value = "ComplianceOfficer")]
    ComplianceOfficer
};
[Table("User")]
[PrimaryKey("UserId")]
public class User
{
    [Key]
    public int UserId { get; set; }
    [Required]
    public string? Name { get; set; }
    [Column(TypeName = "VARCHAR(20)")]
    [Required]
    public UserRole Role { get; set; }//enum UserRole will be used here

    [Column(TypeName = "VARCHAR(50)")]
    [Required, RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
    ErrorMessage = "Invalid email address format.")]
    public string? Email { get; set; }
    [Column(TypeName = "VARCHAR(10)")]
    [Required, RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits.")]
    public string? Phone { get; set; }
    public bool Status { get; set; }
    [Required]
    [Column(TypeName = "varchar(max)")]
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; }
    //-------------------PrimaryKey---------------------//
    public List<Recommendation> Recommendations { get; set; }
    public List<Application> Applications { get; set; } = new List<Application>();
    public List<Review> Reviews { get; set; } = new List<Review>();

    public List<Decision> Decisions { get; set; } = new List<Decision>();
    // public virtual ICollection<User> Users{get; set;}=new List<User>(); 
    //-------------------ForeignKey---------------------//

    public List<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public List<Notification> Notifications { get; set; } = new List<Notification>();

    public List<Report> Reports { get; set; } = new List<Report>();
}
