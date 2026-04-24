namespace GrantTrack.Events
{
    /// <summary>
    /// This event is fired whenever a grantee submits evidence.
    /// It captures metadata for auditing and notifications.
    /// </summary>
    public class GrantReportSubmittedEvent
    {
        public int ReportId { get; set; }
        public int ApplicationId { get; set; }
        public string EvidencePath { get; set; }
        public string SubmittedBy { get; set; } // Current User ID or Name
        public DateTime OccurredOn { get; set; }

        public GrantReportSubmittedEvent(int reportId, int appId, string path, string user)
        {
            ReportId = reportId;
            ApplicationId = appId;
            EvidencePath = path;
            SubmittedBy = user;
            OccurredOn = DateTime.UtcNow;
        }
    }
}