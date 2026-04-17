using System.ComponentModel.DataAnnotations;

namespace GrantTrack.Dto.GrantReportDtos
{
    public class CreateGrantReportDto
    {
        // The ID of the application for which the report is being submitted
        [Required]
        public int ApplicationId { get; set; }

        // Short description or title of the report scope
        [Required]
        [StringLength(200)]
        public string Scope { get; set; }

        // Detailed metrics or content of the report
        [Required]
        public string Metrics { get; set; }

        // Path or URL to the evidence document (S3/Azure Blob link)
        public string? EvidenceDocumentPath { get; set; }
    }
}