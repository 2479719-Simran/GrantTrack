using GrantTrack.Dto.GrantReportDtos;

namespace GrantTrack.Service.GrantReportServices;

public interface IGrantReportService
{
    /// <summary>
    /// Creates a new grant report or updates the existing one for this
    /// application. Status is always set to Submitted on save (a Returned
    /// report being re-submitted moves back to Submitted; a Verified
    /// report cannot be modified).
    /// </summary>
    Task<GrantReportResponseDto> UpsertAsync(int currentUserId, GrantReportRequestDto dto);

    /// <summary>
    /// Returns the latest grant report for an application, or null when
    /// none exists. <paramref name="privilegedReader"/>=true means the
    /// caller is Admin / Approver / Finance / Compliance — they get blanket
    /// read; otherwise the caller must be the owning applicant.
    /// </summary>
    Task<GrantReportResponseDto?> GetByApplicationAsync(int applicationId, int currentUserId, bool privilegedReader);
}
