using System;
using GrantTrack.Domain.Entities;
namespace GrantTrack.Dto.DecisionDtos;
public class DecisionHistoryDto
{
        public int DecisionId { get; set; }
        public DecisionStatus Status { get; set; }
        public string ApproverName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
}
