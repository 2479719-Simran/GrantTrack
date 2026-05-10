using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ApproverDtos;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.ApproverServices;

public class ApproverService : IApproverService
{
    private readonly GrantTrackDbContext _db;

    public ApproverService(GrantTrackDbContext db)
    {
        _db = db;
    }

    public async Task<List<AwaitingDecisionDto>> GetAwaitingDecisionsAsync()
    {
        // "Awaiting" = has at least one Recommendation, but no Decision yet.
        // We do most of the aggregation inline so the controller gets ready-to-render rows.
        var recsByApp = await _db.Recommendations
            .GroupBy(r => r.ApplicationId)
            .Select(g => new
            {
                ApplicationId = g.Key,
                Total = g.Count(),
                Approved = g.Count(r => r.Decision == ReviewDecision.Approved),
                Rejected = g.Count(r => r.Decision == ReviewDecision.Rejected),
            })
            .ToListAsync();

        if (recsByApp.Count == 0) return new();

        var appIds = recsByApp.Select(x => x.ApplicationId).ToList();

        // Decided ids — anything in here is filtered out.
        var decidedAppIds = await _db.Decisions
            .Where(d => appIds.Contains(d.ApplicationId))
            .Select(d => d.ApplicationId)
            .ToListAsync();
        var decidedSet = decidedAppIds.ToHashSet();

        // Average review score per application — joins onto Review (the
        // reviewer's score lives there, not on Recommendation).
        var scoresByApp = await _db.Reviews
            .Where(r => appIds.Contains(r.ApplicationId))
            .GroupBy(r => r.ApplicationId)
            .Select(g => new
            {
                ApplicationId = g.Key,
                AvgScore = g.Average(r => (double?)r.Score) ?? 0d,
            })
            .ToDictionaryAsync(x => x.ApplicationId, x => x.AvgScore);

        // Hydrate applicant + program names in one shot.
        var apps = await _db.Applications
            .Where(a => appIds.Contains(a.ApplicationId) && !decidedSet.Contains(a.ApplicationId))
            .Select(a => new
            {
                a.ApplicationId,
                a.ApplicantId,
                ApplicantName = a.ApplicantIDNavigation != null ? a.ApplicantIDNavigation.Name : null,
                ApplicantEmail = a.ApplicantIDNavigation != null ? a.ApplicantIDNavigation.Email : null,
                a.ProgramId,
                ProgramName = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.Name : null,
                a.SubmittedDate,
                a.Status,
            })
            .OrderBy(a => a.SubmittedDate)
            .ToListAsync();

        return apps.Select(a =>
        {
            var counts = recsByApp.First(x => x.ApplicationId == a.ApplicationId);
            scoresByApp.TryGetValue(a.ApplicationId, out var avg);
            return new AwaitingDecisionDto
            {
                ApplicationId = a.ApplicationId,
                ApplicantId = a.ApplicantId,
                ApplicantName = a.ApplicantName ?? string.Empty,
                ApplicantEmail = a.ApplicantEmail ?? string.Empty,
                ProgramId = a.ProgramId,
                ProgramName = a.ProgramName ?? string.Empty,
                SubmittedDate = a.SubmittedDate,
                Status = a.Status.ToString(),
                RecommendationCount = counts.Total,
                ApprovedCount = counts.Approved,
                RejectedCount = counts.Rejected,
                AverageScore = Math.Round(avg, 2),
            };
        }).ToList();
    }

    public async Task<List<ApproverRecommendationDto>> GetRecommendationsForAsync(int applicationId)
    {
        // Pull recommendations with reviewer info up-front.
        var recs = await _db.Recommendations
            .Where(r => r.ApplicationId == applicationId)
            .Select(r => new
            {
                r.RecommendationId,
                r.ApplicationId,
                r.ReviewerId,
                ReviewerName = r.ReviewerIdNavigation != null ? r.ReviewerIdNavigation.Name : null,
                ReviewerEmail = r.ReviewerIdNavigation != null ? r.ReviewerIdNavigation.Email : null,
                r.Decision,
                r.Notes,
                r.Date,
            })
            .OrderByDescending(r => r.Date)
            .ToListAsync();

        if (recs.Count == 0) return new();

        // Pull the matching Review rows so we can surface score + comments.
        var reviewerIds = recs.Select(r => r.ReviewerId).Distinct().ToList();
        var reviews = await _db.Reviews
            .Where(rv => rv.ApplicationId == applicationId && reviewerIds.Contains(rv.ReviewerId))
            .Select(rv => new { rv.ReviewerId, rv.Score, rv.Comments })
            .ToListAsync();
        var reviewByReviewer = reviews
            .GroupBy(rv => rv.ReviewerId)
            // If a reviewer has more than one review row (shouldn't happen
            // given the bulk-assign duplicate guard), prefer the one with
            // the highest score so the approver sees the strongest signal.
            .ToDictionary(g => g.Key, g => g.OrderByDescending(rv => rv.Score).First());

        return recs.Select(r =>
        {
            reviewByReviewer.TryGetValue(r.ReviewerId, out var review);
            return new ApproverRecommendationDto
            {
                RecommendationId = r.RecommendationId,
                ApplicationId = r.ApplicationId,
                ReviewerId = r.ReviewerId,
                ReviewerName = r.ReviewerName ?? string.Empty,
                ReviewerEmail = r.ReviewerEmail ?? string.Empty,
                Decision = r.Decision,
                RecommendationNotes = r.Notes ?? string.Empty,
                Score = review?.Score ?? 0,
                ReviewComments = review?.Comments ?? string.Empty,
                Date = r.Date,
            };
        }).ToList();
    }
}
