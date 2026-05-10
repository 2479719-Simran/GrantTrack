using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ComplianceDtos;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.ComplianceListServices;

public class ComplianceListService : IComplianceListService
{
    private readonly GrantTrackDbContext _db;

    public ComplianceListService(GrantTrackDbContext db)
    {
        _db = db;
    }

    public async Task<List<ComplianceApplicationListDto>> ListApprovedAsync()
    {
        // Step 1: anchor on Decisions with value Approved — only those apps
        // are eligible for compliance checks (BE rule in ComplianceCheckService).
        var approved = await _db.Decisions
            .Where(d => d.DecisionValue == DecisionStatus.Approved)
            .GroupBy(d => d.ApplicationId)
            .Select(g => new
            {
                ApplicationId = g.Key,
                LatestDate = g.Max(x => x.Date),
            })
            .ToListAsync();

        if (approved.Count == 0) return new();

        var appIds = approved.Select(x => x.ApplicationId).ToList();
        var decisionDateByApp = approved.ToDictionary(x => x.ApplicationId, x => x.LatestDate);

        // Step 2: applicant + program info.
        var apps = await _db.Applications
            .Where(a => appIds.Contains(a.ApplicationId))
            .Select(a => new
            {
                a.ApplicationId,
                a.ApplicantId,
                ApplicantName = a.ApplicantIDNavigation != null ? a.ApplicantIDNavigation.Name : null,
                ApplicantEmail = a.ApplicantIDNavigation != null ? a.ApplicantIDNavigation.Email : null,
                a.ProgramId,
                ProgramName = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.Name : null,
            })
            .ToListAsync();

        // Step 3: every compliance check for these apps. Project just what
        // we need to compute rollups.
        var checks = await _db.ComplianceChecks
            .Where(c => appIds.Contains(c.ApplicationId))
            .Select(c => new { c.ApplicationId, c.Type, c.Result })
            .ToListAsync();
        var checksByApp = checks
            .GroupBy(c => c.ApplicationId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Step 4: GrantReport status per app. Used by both the count rollups
        // and the Stage derivation. We pick the latest report per app in case
        // there's ever more than one.
        var reports = await _db.GrantReports
            .Where(r => appIds.Contains(r.ApplicationId))
            .Select(r => new { r.ApplicationId, r.Status, r.SubmittedDate })
            .ToListAsync();
        var reportByApp = reports
            .GroupBy(r => r.ApplicationId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.SubmittedDate).First());

        // Step 5: stitch.
        return apps.Select(a =>
        {
            checksByApp.TryGetValue(a.ApplicationId, out var appChecks);
            appChecks ??= new();
            reportByApp.TryGetValue(a.ApplicationId, out var report);
            decisionDateByApp.TryGetValue(a.ApplicationId, out var decisionDate);

            var reportStatus = report?.Status.ToString();

            // Derive per-check stage to compute the rollups: a check is
            // "Verified" only when the report is Verified, "Returned" only
            // when the report is Returned, otherwise it's still Scheduled.
            int verified = 0, flagged = 0, scheduled = 0;
            foreach (var c in appChecks)
            {
                var stage = StageOf(c.Result, report?.Status);
                if (stage == "Verified") verified++;
                else if (stage == "Returned") flagged++;
                else scheduled++;
            }

            return new ComplianceApplicationListDto
            {
                ApplicationId = a.ApplicationId,
                ApplicantId = a.ApplicantId,
                ApplicantName = a.ApplicantName ?? string.Empty,
                ApplicantEmail = a.ApplicantEmail ?? string.Empty,
                ProgramId = a.ProgramId,
                ProgramName = a.ProgramName ?? string.Empty,
                DecisionDate = decisionDate,
                HasFinancialCheck = appChecks.Any(c => c.Type == ComplianceType.Financial),
                HasOperationalCheck = appChecks.Any(c => c.Type == ComplianceType.Operational),
                CheckCount = appChecks.Count,
                VerifiedCount = verified,
                FlaggedCount = flagged,
                ScheduledCount = scheduled,
                ReportStatus = reportStatus,
            };
        })
        .OrderByDescending(x => x.DecisionDate)
        .ToList();
    }

    public async Task<ComplianceApplicationDetailDto?> GetDetailAsync(int applicationId)
    {
        // Header — application + applicant + program in one query.
        var head = await _db.Applications
            .Where(a => a.ApplicationId == applicationId)
            .Select(a => new
            {
                a.ApplicationId,
                a.Status,
                a.SubmittedDate,
                a.ApplicantId,
                ApplicantName = a.ApplicantIDNavigation != null ? a.ApplicantIDNavigation.Name : null,
                ApplicantEmail = a.ApplicantIDNavigation != null ? a.ApplicantIDNavigation.Email : null,
                ApplicantPhone = a.ApplicantIDNavigation != null ? a.ApplicantIDNavigation.Phone : null,
                a.ProgramId,
                ProgramName = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.Name : null,
                ProgramDescription = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.Description : null,
                ProgramBudget = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.Budget : 0m,
                ProgramStartDate = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.StartDate : default,
                ProgramEndDate = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.EndDate : default,
            })
            .FirstOrDefaultAsync();

        if (head is null) return null;

        // Most recent Approved decision.
        var decision = await _db.Decisions
            .Where(d => d.ApplicationId == applicationId
                     && d.DecisionValue == DecisionStatus.Approved)
            .OrderByDescending(d => d.Date)
            .Select(d => new
            {
                d.UserId,
                ApproverName = d.User != null ? d.User.Name : null,
                d.Notes,
                d.Date,
            })
            .FirstOrDefaultAsync();

        // Latest GrantReport (if any).
        var report = await _db.GrantReports
            .Where(r => r.ApplicationId == applicationId)
            .OrderByDescending(r => r.SubmittedDate)
            .Select(r => new
            {
                r.GrantReportId,
                r.Status,
                r.Scope,
                r.Notes,
                r.SubmittedDate,
            })
            .FirstOrDefaultAsync();

        // All compliance checks for this app, in chronological order.
        var rawChecks = await _db.ComplianceChecks
            .Where(c => c.ApplicationId == applicationId)
            .OrderBy(c => c.Date)
            .Select(c => new
            {
                c.CheckId,
                c.ApplicationId,
                c.Type,
                c.Result,
                c.Date,
                c.Notes,
            })
            .ToListAsync();

        var checks = rawChecks.Select(c => new ComplianceCheckInfoDto
        {
            CheckId = c.CheckId,
            ApplicationId = c.ApplicationId,
            Type = c.Type.ToString(),
            Result = c.Result.ToString(),
            Date = c.Date,
            Notes = c.Notes ?? string.Empty,
            Stage = StageOf(c.Result, report?.Status),
        }).ToList();

        return new ComplianceApplicationDetailDto
        {
            ApplicationId = head.ApplicationId,
            Status = head.Status.ToString(),
            SubmittedDate = head.SubmittedDate,
            ApplicantId = head.ApplicantId,
            ApplicantName = head.ApplicantName ?? string.Empty,
            ApplicantEmail = head.ApplicantEmail ?? string.Empty,
            ApplicantPhone = head.ApplicantPhone ?? string.Empty,
            ProgramId = head.ProgramId,
            ProgramName = head.ProgramName ?? string.Empty,
            ProgramDescription = head.ProgramDescription ?? string.Empty,
            ProgramBudget = head.ProgramBudget,
            ProgramStartDate = head.ProgramStartDate,
            ProgramEndDate = head.ProgramEndDate,
            ApproverId = decision?.UserId,
            ApproverName = decision?.ApproverName,
            DecisionNotes = decision?.Notes,
            DecisionDate = decision?.Date,
            GrantReportId = report?.GrantReportId,
            ReportStatus = report?.Status.ToString(),
            ReportScope = report?.Scope,
            ReportNotes = report?.Notes,
            ReportSubmittedDate = report?.SubmittedDate,
            Checks = checks,
        };
    }

    /// <summary>
    /// Maps a (check.Result, GrantReport.Status) pair into the FE-friendly
    /// stage label. The BE creates fresh checks with Result=Flagged (its
    /// "default before review" — see ComplianceCheckService.cs:51), which
    /// is identical to a check that was reviewed and found non-compliant.
    /// We disambiguate via the report status: a check is only truly
    /// finalised when the report has been Verified or Returned.
    /// </summary>
    private static string StageOf(ComplianceResult checkResult, ReportStatus? reportStatus)
    {
        return reportStatus switch
        {
            ReportStatus.Verified => "Verified",
            ReportStatus.Returned => "Returned",
            _ => "Scheduled",
        };
    }
}
