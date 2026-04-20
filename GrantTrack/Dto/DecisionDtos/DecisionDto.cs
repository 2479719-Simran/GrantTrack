using System.ComponentModel.DataAnnotations;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Dto.DecisionDtos
{
    /// <summary>
    /// Data transfer object for creating a grant application decision.
    /// </summary>
    public class DecisionDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the application.
        /// </summary>
        [Required]
        public int ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the decision value (Approved or Rejected).
        /// </summary>
        [Required]
        [EnumDataType(typeof(DecisionStatus))]
        public DecisionStatus DecisionValue { get; set; }

        /// <summary>
        /// Gets or sets the justification or notes for the decision.
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date of the decision.
        /// </summary>
        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}