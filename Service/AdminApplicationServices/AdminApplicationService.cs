using GrantTrack.Domain.Entities;
using GrantTrack.Dto.AdminDtos;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.AdminApplicationServices;

public class AdminApplicationService : IAdminApplicationService
{
    private readonly GrantTrackDbContext _db;

    public AdminApplicationService(GrantTrackDbContext db)
    {
        _db = db;
    }

    public async Task<List<AdminApplicationListDto>> ListAllAsync()
    {
        // 1. Pull every application with its applicant + program in one query.
        var apps = await _db.Applications
            .Select(a => new
            {
                a.ApplicationId,
                a.ApplicantId,
                ApplicantName = a.ApplicantIDNavigation != null ? a.ApplicantIDNavigation.Name : null,
                ApplicantEmail = a.ApplicantIDNavigation != null ? a.ApplicantIDNavigation.Email : null,
                a.ProgramId,
                ProgramName = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.Name : null,
                a.Status,
                a.SubmittedDate,
            })
            .OrderByDescending(a => a.SubmittedDate)
            .ToListAsync();

        if (apps.Count == 0) return new();

        var appIds = apps.Select(a => a.ApplicationId).ToList();

        // 2. Roll up counts per app — three small projections, one ToDictionary each.
        var reviewerCounts = await _db.Reviews
            .Where(r => appIds.Contains(r.ApplicationId))
            .GroupBy(r => r.ApplicationId)
            .Select(g => new { ApplicationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ApplicationId, x => x.Count);

        var recCounts = await _db.Recommendations
            .Where(r => appIds.Contains(r.ApplicationId))
            .GroupBy(r => r.ApplicationId)
            .Select(g => new { ApplicationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ApplicationId, x => x.Count);

        // The Documents schema has both ApplicantionId (typo column) and a
        // shadow FK ApplicationId — we group by the typo'd column because
        // it's directly mapped on the entity.
        var docCounts = await _db.Documents
            .Where(d => appIds.Contains(d.ApplicantionId))
            .GroupBy(d => d.ApplicantionId)
            .Select(g => new { ApplicationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ApplicationId, x => x.Count);

        var decisions = await _db.Decisions
            .Where(d => appIds.Contains(d.ApplicationId))
            .Select(d => new
            {
                d.ApplicationId,
                d.DecisionValue,
                d.Date,
            })
            .ToListAsync();
        // If somehow there's more than one decision per app, we surface the
        // most recent one. Shouldn't happen given the BE rejects re-decisions.
        var decisionByApp = decisions
            .GroupBy(d => d.ApplicationId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.Date).First());

        return apps.Select(a =>
        {
            reviewerCounts.TryGetValue(a.ApplicationId, out var rev);
            recCounts.TryGetValue(a.ApplicationId, out var rec);
            docCounts.TryGetValue(a.ApplicationId, out var doc);
            decisionByApp.TryGetValue(a.ApplicationId, out var dec);
            return new AdminApplicationListDto
            {
                ApplicationId = a.ApplicationId,
                ApplicantId = a.ApplicantId,
                ApplicantName = a.ApplicantName ?? string.Empty,
                ApplicantEmail = a.ApplicantEmail ?? string.Empty,
                ProgramId = a.ProgramId,
                ProgramName = a.ProgramName ?? string.Empty,
                Status = a.Status.ToString(),
                SubmittedDate = a.SubmittedDate,
                ReviewerCount = rev,
                RecommendationCount = rec,
                DocumentCount = doc,
                FinalDecision = dec?.DecisionValue.ToString(),
                DecisionDate = dec?.Date,
            };
        }).ToList();
    }

    public async Task<AdminApplicationDetailDto?> GetDetailAsync(int applicationId)
    {
        // Header — application + applicant + program in one roundtrip.
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
                ProgramStartDate = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.StartDate : default,
                ProgramEndDate = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.EndDate : default,
                ProgramBudget = a.ProgramIDNavigation != null ? a.ProgramIDNavigation.Budget : 0m,
            })
            .FirstOrDefaultAsync();

        if (head is null) return null;

        // Reviewers assigned to this application + their score/comments + a
        // flag for whether they actually submitted a recommendation yet.
        var reviewerRows = await _db.Reviews
            .Where(r => r.ApplicationId == applicationId)
            .Select(r => new
            {
                r.ReviewId,
                r.ReviewerId,
                ReviewerName = r.ReviewerIDNavigation != null ? r.ReviewerIDNavigation.Name : null,
                ReviewerEmail = r.ReviewerIDNavigation != null ? r.ReviewerIDNavigation.Email : null,
                r.Score,
                r.Comments,
                AssignedDate = r.Date,
            })
            .ToListAsync();

        var submittedReviewerIds = await _db.Recommendations
            .Where(r => r.ApplicationId == applicationId)
            .Select(r => r.ReviewerId)
            .Distinct()
            .ToListAsync();
        var submittedSet = submittedReviewerIds.ToHashSet();

        var reviewers = reviewerRows.Select(r => new AdminReviewerInfoDto
        {
            ReviewId = r.ReviewId,
            ReviewerId = r.ReviewerId,
            ReviewerName = r.ReviewerName ?? string.Empty,
            ReviewerEmail = r.ReviewerEmail ?? string.Empty,
            Score = r.Score,
            Comments = r.Comments ?? string.Empty,
            AssignedDate = r.AssignedDate,
            HasSubmittedRecommendation = submittedSet.Contains(r.ReviewerId),
        }).ToList();

        // Recommendations submitted for this application.
        var recommendations = await _db.Recommendations
            .Where(r => r.ApplicationId == applicationId)
            .Select(r => new AdminRecommendationInfoDto
            {
                RecommendationId = r.RecommendationId,
                ReviewerId = r.ReviewerId,
                ReviewerName = r.ReviewerIdNavigation != null ? r.ReviewerIdNavigation.Name ?? string.Empty : string.Empty,
                Decision = r.Decision,
                Notes = r.Notes ?? string.Empty,
                Date = r.Date,
            })
            .OrderByDescending(r => r.Date)
            .ToListAsync();

        // Final decision (most recent if multiple — shouldn't happen).
        var decisionRow = await _db.Decisions
            .Where(d => d.ApplicationId == applicationId)
            .OrderByDescending(d => d.Date)
            .Select(d => new
            {
                d.DecisionId,
                d.UserId,
                ApproverName = d.User != null ? d.User.Name : null,
                ApproverEmail = d.User != null ? d.User.Email : null,
                d.DecisionValue,
                d.Notes,
                d.Date,
            })
            .FirstOrDefaultAsync();

        AdminDecisionInfoDto? decision = decisionRow is null ? null : new AdminDecisionInfoDto
        {
            DecisionId = decisionRow.DecisionId,
            ApproverId = decisionRow.UserId,
            ApproverName = decisionRow.ApproverName ?? string.Empty,
            ApproverEmail = decisionRow.ApproverEmail ?? string.Empty,
            DecisionValue = decisionRow.DecisionValue.ToString(),
            Notes = decisionRow.Notes ?? string.Empty,
            Date = decisionRow.Date,
        };

        // Documents for this application — uses the typo'd column.
        var docs = await _db.Documents
            .Where(d => d.ApplicantionId == applicationId)
            .Select(d => new AdminDocumentInfoDto
            {
                DocumentId = d.DocumentId,
                DocType = d.DocType,
                FileName = ParseOriginalFileName(d.FileURI),
                DownloadUrl = $"/api/v1/applications/{applicationId}/documents/{d.DocumentId}/download",
            })
            .ToListAsync();

        return new AdminApplicationDetailDto
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
            ProgramStartDate = head.ProgramStartDate,
            ProgramEndDate = head.ProgramEndDate,
            ProgramBudget = head.ProgramBudget,
            Reviewers = reviewers,
            Recommendations = recommendations,
            Decision = decision,
            Documents = docs,
        };
    }

    /// <summary>
    /// Mirrors the helper in DocumentService — extracts the original filename
    /// from the FileURI of "{appId}/{guid}_{originalName}".
    /// </summary>
    private static string ParseOriginalFileName(string fileUri)
    {
        var leaf = Path.GetFileName(fileUri);
        var underscore = leaf.IndexOf('_');
        return underscore >= 0 && underscore < leaf.Length - 1
            ? leaf[(underscore + 1)..]
            : leaf;
    }
}
