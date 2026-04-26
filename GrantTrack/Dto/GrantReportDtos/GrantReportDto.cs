using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; 

namespace GrantTrack.Dto.GrantReportDtos
{
    public class GrantReportDto
    {
        [Required(ErrorMessage = "CheckId is mandatory to link this report to a specific compliance condition.")]
        public int CheckId { get; set; } 
        [Required(ErrorMessage = "Scope is required.")]
        [StringLength(200)]
        public string Scope { get; set; }
        [Required(ErrorMessage = "Metrics are required.")]
        public string Metrics { get; set; }
        [Required(ErrorMessage = "Evidence file (PDF/Image) is mandatory.")]
        public IFormFile EvidenceFile { get; set; } 
    }
}