using System;

namespace GrantTrack.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class Application
{
    [Key]
    public int ApplicationId { get; set; }

    [Required]
    public int ProgramId { get; set; }

    [ForeignKey(nameof(ProgramId))]
    public virtual Program ProgramIDNavigation { get; set; }

    [Required]
    [ForeignKey("ApplicantIDNavigation")]
    public int ApplicantId { get; set; }

    public User?  ApplicantIDNavigation { get; set; } 

    [Required]
    public DateTime SubmittedDate { get; set; }

    [Required]
    public bool Status { get; set; }

    public List<Document> Documents { get; set; } = new List<Document>();
    public List<ApplicationValidation> ApplicationValidations { get; set; } = new List<ApplicationValidation>(); 
    public List<Recommendation> Recommendations { get; set; } = new List<Recommendation>(); 

    public List<Review> Reviews { get; set; } = new List<Review>();     
    public List<Decision> Decisions { get; set; } = new List<Decision>();   

    public List<Notification> Notifications { get; set; }  = new List<Notification>(); 
    public List<GrantReport> GrantReports { get; set; }  = new List<GrantReport>(); 
    public List<ComplianceCheck> ComplianceChecks  { get; set; }  = new List<ComplianceCheck>(); 

    public List<Disbursement> Disbursements  { get; set; }  = new List<Disbursement>(); 




        
}



