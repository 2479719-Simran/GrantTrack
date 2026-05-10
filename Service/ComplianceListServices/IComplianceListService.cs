using GrantTrack.Dto.ComplianceDtos;

namespace GrantTrack.Service.ComplianceListServices;

public interface IComplianceListService
{
    /// <summary>
    /// All approved applications, hydrated with applicant + program info,
    /// per-type check completion flags, check status counts, and (when one
    /// exists) the GrantReport status.
    /// </summary>
    Task<List<ComplianceApplicationListDto>> ListApprovedAsync();

    /// <summary>
    /// Full detail for a single application — header + decision context +
    /// grant report state + every compliance check (with derived Stage).
    /// Returns null if the application doesn't exist.
    /// </summary>
    Task<ComplianceApplicationDetailDto?> GetDetailAsync(int applicationId);
}
