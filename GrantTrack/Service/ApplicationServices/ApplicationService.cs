using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ApplicationDtos;
using GrantTrack.Events;
using GrantTrack.Repository.ApplicationRepositories;
using GrantTrack.Utility;

namespace GrantTrack.Service.ApplicationServices;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _ApplicationRepo;
    private readonly IEventPublisher _eventPublisher;
    public ApplicationService(IApplicationRepository repo, IEventPublisher eventPublisher)
    {
        _ApplicationRepo = repo;
        _eventPublisher = eventPublisher;
    }

    public async Task<ApplicationResponseDto> CreateDraftAsync(CreateApplicationDto dto, int applicantId)
    {
        var application = new Application
        {
            ProgramId = dto.ProgramId,
            ApplicantId = applicantId,
            Status = ApplicationStatus.Draft,
        };

        var created = await _ApplicationRepo.CreateAsync(application);
        return ToDto(created);
    }

    public async Task<ApplicationResponseDto> SubmitAsync(int applicationId, int applicantId)
    {
        var application = await _ApplicationRepo.GetByIdAsync(applicationId)
            ?? throw new KeyNotFoundException(Messages.ApplicationNotFound);

        if (application.ApplicantId != applicantId)
            throw new UnauthorizedAccessException(Messages.Forbidden);

        if (application.Status != ApplicationStatus.Draft)
            throw new InvalidOperationException(Messages.ApplicationNotInDraft);

        application.Status = ApplicationStatus.Submitted;
        application.SubmittedDate = DateTime.UtcNow;
        var updated = await _ApplicationRepo.UpdateAsync(application);
        // Emit Application.Submitted event
        await _eventPublisher.PublishAsync(new ApplicationSubmittedEvent
        {
            ApplicationId = updated.ApplicationId,
            ProgramId = updated.ProgramId,
            ApplicantId = updated.ApplicantId,
            SubmittedAt = updated.SubmittedDate
        });
        return ToDto(updated);
    }

    private static ApplicationResponseDto ToDto(Application a) => new()
    {
        ApplicationId = a.ApplicationId,
        ProgramId = a.ProgramId,
        ApplicantId = a.ApplicantId,
        Status = a.Status.ToString(),
        SubmittedDate = a.SubmittedDate,
    };
}
