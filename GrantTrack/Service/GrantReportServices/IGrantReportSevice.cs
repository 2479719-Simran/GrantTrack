using GrantTrack.Domain.Entities;
using GrantTrack.Dto.GrantReportDtos;

namespace GrantTrack.Service.GrantReportServices
{
    /// <summary>
    /// Service interface for handling Grant Report operations.
    /// Manages file stream submissions and retrieval linked to specific compliance checks.
    /// </summary>
    public interface IGrantReportService
    {
        /// <summary>
        /// Validates the compliance check status and saves the report metadata 
        /// along with the file stream (byte array) into the database.
        /// </summary>
        Task<GrantReport> SubmitReportAsync(GrantReportDto dto);

        /// <summary>
        /// Retrieves the report and evidence file linked to a specific Compliance Check ID.
        /// This allows officers to verify documents for a particular condition (Financial/Operational).
        /// </summary>
        Task<GrantReport?> GetReportByCheckIdAsync(int checkId);

        /// <summary>
        /// Retrieves a specific report by its unique primary key.
        /// </summary>
        Task<GrantReport?> GetReportByIdAsync(int reportId);
    }
}