using System;
using System.Security.AccessControl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace GrantTrack.Domain.Entities;

public class GrantTrackDbContext : DbContext 
{ 
    public DbSet<Operation> Operations { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ComplianceCheck> ComplianceChecks{get; set;}
    public DbSet <GrantReport> GrantReports{get; set;}
    public DbSet<Report> Reports{get; set;}
    public DbSet <Notification>notifications { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=LTIN718866\\SQLEXPRESS;Database=GrantTrack;Trusted_Connection=True;TrustServerCertificate=True");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
    }
}

