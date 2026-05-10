using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ApplicationDtos;
using GrantTrack.Events;
using GrantTrack.Repository.ApplicationRepositories;
using GrantTrack.Repository.Interface;
using GrantTrack.Repository.ProgramRepository;
using GrantTrack.Utility;
using AppEntity = GrantTrack.Domain.Entities.Application;
namespace GrantTrack.Service.ApplicationServices;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _ApplicationRepo;
    private readonly IEventPublisher _eventPublisher;
    private readonly IProgramRepository _programRepo;
    public ApplicationService(IApplicationRepository repo, IEventPublisher eventPublisher , IProgramRepository programRepo)
    {
        _ApplicationRepo = repo;
        _eventPublisher = eventPublisher;
        _programRepo = programRepo;
    }

     public async Task<ApplicationResponseDto> CreateDraftAsync(CreateApplicationDto dto, int applicantId)
    {
        // Guard 1: Program must exist → 404
        if (!await _programRepo.ExistsAsync(dto.ProgramId))
            throw new KeyNotFoundException(Messages.ProgramNotFound);

        // Guard 2: Program must be active → 409
        if (!await _programRepo.IsActiveAsync(dto.ProgramId))
            throw new InvalidOperationException(Messages.ProgramNotActive);

        // Guard 3: No duplicate application for this applicant+program → 409
        if (await _ApplicationRepo.ExistsForApplicantAsync(applicantId, dto.ProgramId))
            throw new InvalidOperationException(Messages.DuplicateApplication);

        var application = new AppEntity
        {
            ProgramId   = dto.ProgramId,
            ApplicantId = applicantId,
            Status      = ApplicationStatus.Draft,
        };

        var created = await _ApplicationRepo.CreateAsync(application);
        return ToDto(created);
    }

    public async Task<ApplicationResponseDto> GetByIdAsync(int applicationId, int applicantId)
    {
        var application = await _ApplicationRepo.GetByIdAsync(applicationId)
            ?? throw new KeyNotFoundException(Messages.ApplicationNotFound);

        if (application.ApplicantId != applicantId)
            throw new UnauthorizedAccessException(Messages.Forbidden);

        return ToDto(application);
    }

    public async Task<ApplicationResponseDto> SubmitAsync(int applicationId, int applicantId)
    {
        var application = await _ApplicationRepo.GetByIdAsync(applicationId)
            ?? throw new KeyNotFoundException(Messages.ApplicationNotFound);

        if (application.ApplicantId != applicantId)
            throw new UnauthorizedAccessException(Messages.Forbidden);

        if (application.Status != ApplicationStatus.Draft)
            throw new InvalidOperationException(Messages.ApplicationNotInDraft);

        application.Status        = ApplicationStatus.Submitted;
        application.SubmittedDate = DateTime.UtcNow;

        var updated = await _ApplicationRepo.UpdateAsync(application);

        await _eventPublisher.PublishAsync(new ApplicationSubmittedEvent
        {
            ApplicationId = updated.ApplicationId,
            ProgramId     = updated.ProgramId,
            ApplicantId   = updated.ApplicantId,
            SubmittedAt   = updated.SubmittedDate
            // !.Value 
        });

        return ToDto(updated);
    }

    private static ApplicationResponseDto ToDto(AppEntity a) => new()
    {
        ApplicationId = a.ApplicationId,
        ProgramId     = a.ProgramId,
        ApplicantId   = a.ApplicantId,
        Status        = a.Status.ToString(),
        SubmittedDate = a.SubmittedDate,
    };

}
