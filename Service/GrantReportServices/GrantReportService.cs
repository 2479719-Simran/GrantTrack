using GrantTrack.Domain.Entities;
using GrantTrack.Dto.GrantReportDtos;
using GrantTrack.Repository.GrantReportRepository;

namespace GrantTrack.Service.GrantReportServices;

public class GrantReportService : IGrantReportService
{
    private readonly IGrantReportRepository _repo;

    public GrantReportService(IGrantReportRepository repo)
    {
        _repo = repo;
    }

    public async Task<GrantReportResponseDto> UpsertAsync(int currentUserId, GrantReportRequestDto dto)
    {
        // 1. Application must exist + be owned by the current user.
        var ownerId = await _repo.GetApplicantIdForApplicationAsync(dto.ApplicationId)
            ?? throw new KeyNotFoundException("Application not found.");
        if (ownerId != currentUserId)
            throw new UnauthorizedAccessException("You don't own this application.");

        // 2. Application must be Approved — otherwise there's no funding
        // to report on. Mirrors the rule the compliance flow assumes.
        if (!await _repo.IsApprovedAsync(dto.ApplicationId))
            throw new InvalidOperationException(
                "A grant report can only be submitted for an approved application.");

        // 3. Look for an existing report — upsert behaviour.
        var existing = await _repo.GetByApplicationIdAsync(dto.ApplicationId);

        if (existing is null)
        {
            var entity = new GrantReport
            {
                ApplicationId = dto.ApplicationId,
                Scope = dto.Scope.Trim(),
                Metrics = dto.Metrics.Trim(),
                Notes = dto.Notes,
                EvidenceDocumentPath = dto.EvidenceDocumentPath,
                Status = ReportStatus.Submitted,
                SubmittedDate = DateTime.UtcNow,
            };
            var saved = await _repo.AddAsync(entity);
            return Map(saved);
        }

        // Verified reports are immutable — once compliance has signed off,
        // the grantee can't re-open it. Mirrors the rule the BE compliance
        // service enforces (see ComplianceCheckService line 86-89).
        if (existing.Status == ReportStatus.Verified)
            throw new InvalidOperationException(
                "This report has already been verified and can no longer be modified.");

        // Update existing — also flips a Returned report back to Submitted
        // so the compliance officer sees it ready for re-review.
        existing.Scope = dto.Scope.Trim();
        existing.Metrics = dto.Metrics.Trim();
        existing.Notes = dto.Notes;
        existing.EvidenceDocumentPath = dto.EvidenceDocumentPath;
        existing.Status = ReportStatus.Submitted;
        existing.SubmittedDate = DateTime.UtcNow;
        await _repo.UpdateAsync(existing);
        return Map(existing);
    }

    public async Task<GrantReportResponseDto?> GetByApplicationAsync(
        int applicationId, int currentUserId, bool privilegedReader)
    {
        if (!privilegedReader)
        {
            // Owner check — only the applicant who owns this app can read.
            var ownerId = await _repo.GetApplicantIdForApplicationAsync(applicationId)
                ?? throw new KeyNotFoundException("Application not found.");
            if (ownerId != currentUserId)
                throw new UnauthorizedAccessException("You don't have access to this report.");
        }

        var report = await _repo.GetByApplicationIdAsync(applicationId);
        return report is null ? null : Map(report);
    }

    private static GrantReportResponseDto Map(GrantReport r) => new()
    {
        GrantReportId = r.GrantReportId,
        ApplicationId = r.ApplicationId,
        Scope = r.Scope,
        Metrics = r.Metrics,
        Status = r.Status.ToString(),
        SubmittedDate = r.SubmittedDate,
        Notes = r.Notes,
        EvidenceDocumentPath = r.EvidenceDocumentPath,
    };
}
