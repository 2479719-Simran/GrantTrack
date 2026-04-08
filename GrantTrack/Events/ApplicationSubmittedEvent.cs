using System;

namespace GrantTrack.Events;

public class ApplicationSubmittedEvent
{
    public int ApplicationId { get; init; }
    public int ProgramId { get; init; }
    public int ApplicantId { get; init; }
    public DateTime SubmittedAt { get; init; }
}
