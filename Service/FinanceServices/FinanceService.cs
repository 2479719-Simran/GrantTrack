using GrantTrack.Domain.Entities;
using GrantTrack.Dto.FinanceDtos;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.FinanceServices;

public class FinanceService : IFinanceService
{
    private readonly GrantTrackDbContext _db;

    public FinanceService(GrantTrackDbContext db)
    {
        _db = db;
    }

    public async Task<List<FinanceApplicationListDto>> ListApprovedAsync()
    {
        // Step 1: anchor on Decisions with value Approved — that's the gate
        // a Finance Officer cares about. We pull the most-recent Decision per
        // application so that if a re-approval ever happens we surface the
        // latest one.
        var approvedDecisions = await _db.Decisions
            .Where(d => d.DecisionValue == DecisionStatus.Approved)
            .GroupBy(d => d.ApplicationId)
            .Select(g => new
            {
                ApplicationId = g.Key,
                LatestDate = g.Max(x => x.Date),
            })
            .ToListAsync();

        if (approvedDecisions.Count == 0) return new();

        var appIds = approvedDecisions.Select(x => x.ApplicationId).ToList();
        var decisionDateByApp = approvedDecisions.ToDictionary(x => x.ApplicationId, x => x.LatestDate);

        // Step 2: applicant + program info for those apps.
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
                ProgramBudget = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.Budget : 0m,
            })
            .ToListAsync();

        // Step 3: roll up disbursements per app — both totals and per-status counts.
        var disbursements = await _db.Disbursements
            .Where(d => appIds.Contains(d.ApplicationId))
            .Select(d => new { d.DisbursementId, d.ApplicationId, d.Amount, d.Status })
            .ToListAsync();
        var disbsByApp = disbursements
            .GroupBy(d => d.ApplicationId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Step 4: roll up payments. We need disbursement-id-to-app mapping
        // so we can attribute each payment back to the right application.
        var disbToApp = disbursements.ToDictionary(d => d.DisbursementId, d => d.ApplicationId);
        var disbursementIds = disbursements.Select(d => d.DisbursementId).ToList();
        var payments = await _db.payments
            .Where(p => disbursementIds.Contains(p.DisbursementId)
                        && p.Status == PaymentStatus.Completed)
            .Select(p => new { p.DisbursementId, p.Amount })
            .ToListAsync();
        var paidByApp = payments
            .Where(p => disbToApp.ContainsKey(p.DisbursementId))
            .GroupBy(p => disbToApp[p.DisbursementId])
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Step 5: stitch.
        return apps.Select(a =>
        {
            disbsByApp.TryGetValue(a.ApplicationId, out var disbs);
            disbs ??= new();
            var totalDisbursed = disbs.Sum(d => d.Amount);
            paidByApp.TryGetValue(a.ApplicationId, out var paid);
            decisionDateByApp.TryGetValue(a.ApplicationId, out var decisionDate);

            return new FinanceApplicationListDto
            {
                ApplicationId = a.ApplicationId,
                ApplicantId = a.ApplicantId,
                ApplicantName = a.ApplicantName ?? string.Empty,
                ApplicantEmail = a.ApplicantEmail ?? string.Empty,
                ProgramId = a.ProgramId,
                ProgramName = a.ProgramName ?? string.Empty,
                ProgramBudget = a.ProgramBudget,
                DecisionDate = decisionDate,
                DisbursementCount = disbs.Count,
                TotalDisbursed = totalDisbursed,
                TotalPaid = paid,
                Remaining = a.ProgramBudget - totalDisbursed,
                PendingCount = disbs.Count(d => d.Status == DisbursementStatus.Pending),
                ScheduledCount = disbs.Count(d => d.Status == DisbursementStatus.Scheduled),
                PaidCount = disbs.Count(d => d.Status == DisbursementStatus.Paid),
                PartiallyPaidCount = disbs.Count(d => d.Status == DisbursementStatus.PartiallyPaid),
                CancelledCount = disbs.Count(d => d.Status == DisbursementStatus.Cancelled),
            };
        })
        .OrderByDescending(x => x.DecisionDate)
        .ToList();
    }

    public async Task<FinanceApplicationDetailDto?> GetDetailAsync(int applicationId)
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

        // Most recent Approved decision, if any. The FO needs to see who
        // approved + their notes for context.
        var decision = await _db.Decisions
            .Where(d => d.ApplicationId == applicationId
                     && d.DecisionValue == DecisionStatus.Approved)
            .OrderByDescending(d => d.Date)
            .Select(d => new
            {
                d.DecisionId,
                d.UserId,
                ApproverName = d.User != null ? d.User.Name : null,
                d.DecisionValue,
                d.Notes,
                d.Date,
            })
            .FirstOrDefaultAsync();

        // Reuse the public list method so the projection logic lives in
        // exactly one place — the new shared endpoint and this detail DTO
        // both produce the same shape.
        var disbursementDtos = await ListDisbursementsForApplicationAsync(applicationId);

        var totalDisbursed = disbursementDtos.Sum(d => d.Amount);
        var totalPaidAcrossApp = disbursementDtos.Sum(d => d.TotalPaid);

        return new FinanceApplicationDetailDto
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
            DecisionId = decision?.DecisionId,
            ApproverId = decision?.UserId,
            ApproverName = decision?.ApproverName,
            DecisionValue = decision?.DecisionValue.ToString(),
            DecisionNotes = decision?.Notes,
            DecisionDate = decision?.Date,
            Disbursements = disbursementDtos,
            TotalDisbursed = totalDisbursed,
            TotalPaid = totalPaidAcrossApp,
            Remaining = head.ProgramBudget - totalDisbursed,
        };
    }

    public async Task<List<FinanceDisbursementDto>> ListDisbursementsForApplicationAsync(int applicationId)
    {
        var disbursements = await _db.Disbursements
            .Where(d => d.ApplicationId == applicationId)
            .OrderBy(d => d.ScheduledDate)
            .Select(d => new
            {
                d.DisbursementId,
                d.Amount,
                d.ScheduledDate,
                d.ActualDate,
                d.Status,
            })
            .ToListAsync();

        if (disbursements.Count == 0) return new();

        var disbursementIds = disbursements.Select(d => d.DisbursementId).ToList();
        var payments = await _db.payments
            .Where(p => disbursementIds.Contains(p.DisbursementId))
            .OrderBy(p => p.Date)
            .Select(p => new
            {
                p.PaymentId,
                p.DisbursementId,
                p.Amount,
                p.Date,
                p.Method,
                p.Status,
            })
            .ToListAsync();

        var paymentsByDisbursement = payments
            .GroupBy(p => p.DisbursementId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return disbursements.Select(d =>
        {
            paymentsByDisbursement.TryGetValue(d.DisbursementId, out var paidRows);
            paidRows ??= new();
            // Only Completed payments count toward TotalPaid — Failed /
            // Cancelled / Pending payments are visible but not aggregated.
            var totalPaid = paidRows.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
            return new FinanceDisbursementDto
            {
                DisbursementId = d.DisbursementId,
                Amount = d.Amount,
                ScheduledDate = d.ScheduledDate,
                ActualDate = d.ActualDate,
                Status = d.Status.ToString(),
                Payments = paidRows.Select(p => new FinancePaymentDto
                {
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    Date = p.Date,
                    Method = p.Method.ToString(),
                    Status = p.Status.ToString(),
                }).ToList(),
                TotalPaid = totalPaid,
                RemainingOnTranche = d.Amount - totalPaid,
            };
        }).ToList();
    }

    public Task<int?> GetApplicantIdAsync(int applicationId) =>
        _db.Applications
            .Where(a => a.ApplicationId == applicationId)
            .Select(a => (int?)a.ApplicantId)
            .FirstOrDefaultAsync();
}
