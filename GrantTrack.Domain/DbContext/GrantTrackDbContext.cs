using System;
using System.Security.AccessControl;
using Microsoft.EntityFrameworkCore;
namespace GrantTrack.Domain.Entities;

public class GrantTrackDbContext : DbContext
{

    public GrantTrackDbContext(DbContextOptions<GrantTrackDbContext> options)
        : base(options)
    {
        
    }
    
    public DbSet<Application> Applications { get; set; }
    public DbSet<Payment> payments { get; set; }
    public DbSet<Disbursement> Disbursements { get; set; }
    public DbSet<ApplicationValidation> ApplicationValidations { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Decision> Decisions { get; set; }
    public DbSet<EligibilityRule> EligibilityRules { get; set; }
    public DbSet<Program> Programs { get; set; }
    public DbSet<RequiredDocument> RequiredDocuments { get; set; }
    public DbSet<Operation> Operations { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Recommendation> Recommendations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ComplianceCheck> ComplianceChecks { get; set; }
    public DbSet<GrantReport> GrantReports { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Notification> notifications { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Application>()
        .HasOne(q => q.ApplicantIDNavigation)
        .WithMany(q => q.Applications)
        .HasForeignKey(a => a.ApplicantId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Application>()
        .HasOne(q => q.ProgramIDNavigation)
        .WithMany(q => q.Applications)
        .HasForeignKey(a => a.ProgramId)
        .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Recommendation>()
        .HasOne(q => q.ApplicationIdNavigation)
        .WithMany(q => q.Recommendations)
        .HasForeignKey(q => q.ApplicationId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Recommendation>()
        .HasOne(q => q.ReviewerIdNavigation)
        .WithMany(q => q.Recommendations)
        .HasForeignKey(q => q.ReviewerId)
        .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Review>()
        .HasOne(q => q.ApplicationIDNavigation)
        .WithMany(q => q.Reviews)
        .HasForeignKey(q => q.ApplicationId)
        .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Review>()
        .HasOne(q => q.ReviewerIDNavigation)
        .WithMany(q => q.Reviews)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Decision>()
        .HasOne(q => q.Application)
        .WithMany(q => q.Decisions)
        .HasForeignKey(q => q.ApplicationId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Decision>()
        .HasOne(q => q.User)
        .WithMany(q => q.Decisions)
        .HasForeignKey(q => q.UserId)
        .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Notification>()
        .HasOne(q => q.ApplicationIdNavigation)
        .WithMany(q => q.Notifications)
        .HasForeignKey(q => q.ApplicationId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
        .HasOne(q => q.UserIdNavigation)
        .WithMany(q => q.Notifications)
        .HasForeignKey(q => q.UserId)
        .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<User>()
        .Property(u => u.Role)
        .HasConversion<string>(); 

        modelBuilder.Entity<Application>()
        .Property(u => u.Status)
        .HasConversion<string>(); 

        modelBuilder.Entity<Decision>()
        .Property(u => u.DecisionValue)
        .HasConversion<string>(); 

    }
}
