using DynamicExpresso;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ApplicationDtos;
using GrantTrack.Dto.DisbursementDtos;
using GrantTrack.Events;
using GrantTrack.Repository.ApplicationRepositories;
using GrantTrack.Repository.ProgramRepository;
using GrantTrack.Utility;

namespace GrantTrack.Service.ApplicationServices;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _ApplicationRepo;
    private readonly IEventPublisher _eventPublisher;
    private readonly IProgramRepository _programRepo;

    public ApplicationService(
        IApplicationRepository repo,
        IEventPublisher eventPublisher,
        IProgramRepository programRepo)
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

        var application = new Application
        {
            ProgramId = dto.ProgramId,
            ApplicantId = applicantId,
            Status = ApplicationStatus.Draft,
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

        //Validate BEFORE changing status
        var validationResults = await ValidateApplicationAsync(application);

        if (validationResults.Count > 0)
            await _ApplicationRepo.AddValidationsAsync(validationResults);

        var anyFailed = validationResults.Any(r => r.Result == "Failed");

        if (anyFailed)
        {
            // Validation failed — stay as Draft, return failure messages
            var failedDto = ToDto(application);
            failedDto.ValidationMessages = validationResults
                .Where(v => v.Result == "Failed")
                .Select(v => new ValidationMessageDto
                {
                    RuleName = v.RuleName ?? string.Empty,
                    Result = v.Result ?? string.Empty,
                    Message = v.Message ?? string.Empty
                })
                .ToList();

            return failedDto;
        }

        //All validations passed — move to UnderReview 
        application.Status = ApplicationStatus.UnderReview;
        application.SubmittedDate = DateTime.UtcNow;
        var updated = await _ApplicationRepo.UpdateAsync(application);

        await _eventPublisher.PublishAsync(new ApplicationSubmittedEvent
        {
            ApplicationId = updated.ApplicationId,
            ProgramId = updated.ProgramId,
            ApplicantId = updated.ApplicantId,
            SubmittedAt = updated.SubmittedDate
        });

        return ToDto(updated);
    }
    /// <summary>
    /// Runs all validations on an application:
    /// 1. Checks mandatory required documents are uploaded.
    /// 2. Evaluates each EligibilityRule.RuleExpression via DynamicExpresso.
    /// Returns a list of ApplicationValidation rows describing pass/fail outcomes.
    /// </summary>
    private async Task<List<ApplicationValidation>> ValidateApplicationAsync(Application application)
    {
        var results = new List<ApplicationValidation>();
        var now = DateTime.UtcNow;

        var appWithDocs = await _ApplicationRepo.GetForEvaluationAsync(application.ApplicationId);
        if (appWithDocs is null) return results;

        //Mandatory document check
        var requiredDocs = await _ApplicationRepo.GetRequiredDocsAsync(application.ProgramId);

        var uploadedTypes = appWithDocs.Documents
            .Where(d => d.UploadedAt.HasValue)
            .Select(d => d.DocType)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var req in requiredDocs)
        {
            if (string.IsNullOrWhiteSpace(req.Name)) continue;

            if (!uploadedTypes.Contains(req.Name))
            {
                results.Add(new ApplicationValidation
                {
                    ApplicationId = application.ApplicationId,
                    RuleName = $"RequiredDoc:{req.Name}",
                    Result = "Failed",
                    Message = $"Mandatory document '{req.Name}' is missing.",
                    CheckedDate = now
                });
            }
            else
            {
                results.Add(new ApplicationValidation
                {
                    ApplicationId = application.ApplicationId,
                    RuleName = $"RequiredDoc:{req.Name}",
                    Result = "Passed",
                    Message = $"Mandatory document '{req.Name}' uploaded.",
                    CheckedDate = now
                });
            }

        }

        //Eligibility rule evaluation (DynamicExpresso)
        var rules = await _ApplicationRepo.GetRulesAsync(application.ProgramId);

        var interpreter = new Interpreter();
        interpreter.SetVariable("ApplicantId", appWithDocs.ApplicantId);
        interpreter.SetVariable("ProgramId", appWithDocs.ProgramId);
        interpreter.SetVariable("DocumentCount", appWithDocs.Documents.Count);
        interpreter.SetVariable("SubmittedDate", appWithDocs.SubmittedDate);
        if (appWithDocs.ApplicantIDNavigation is not null)
        {
            var user = appWithDocs.ApplicantIDNavigation;
            interpreter.SetVariable("AccountActive", user.Status);

            var accountAgeDays = (int)(DateTime.UtcNow - user.CreatedAt).TotalDays;
            interpreter.SetVariable("AccountAge", accountAgeDays);
        }

        foreach (var rule in rules)
        {
            var ruleName = string.IsNullOrWhiteSpace(rule.RuleDescription)
                ? $"Rule#{rule.RuleId}"
                : rule.RuleDescription;

            try
            {
                var passed = interpreter.Eval<bool>(rule.RuleExpression);
                results.Add(new ApplicationValidation
                {
                    ApplicationId = application.ApplicationId,
                    RuleName = ruleName,
                    Result = passed ? "Passed" : "Failed",
                    Message = passed
                        ? "Rule passed"
                        : $"Rule failed: {rule.RuleExpression}",
                    CheckedDate = now
                });
            }
            catch (Exception ex)
            {
                results.Add(new ApplicationValidation
                {
                    ApplicationId = application.ApplicationId,
                    RuleName = ruleName,
                    Result = "Failed",
                    Message = $"Rule error: {ex.Message}",
                    CheckedDate = now
                });
            }
        }

        return results;
    }

    private static ApplicationResponseDto ToDto(Application a) => new()
    {
        ApplicationId = a.ApplicationId,
        ProgramId = a.ProgramId,
        ApplicantId = a.ApplicantId,
        Status = a.Status.ToString(),
        SubmittedDate = a.SubmittedDate,
    };

    public async Task<List<ValidationResponseDto>> GetValidationsAsync(
    int applicationId, int currentUserId, string currentUserRole)
    {
        var application = await _ApplicationRepo.GetByIdAsync(applicationId)
            ?? throw new KeyNotFoundException(Messages.ApplicationNotFound);

        // Applicants can only see their own; Reviewers/Admins can see any
        if (currentUserRole == nameof(UserRole.Applicant) && application.ApplicantId != currentUserId)
            throw new UnauthorizedAccessException(Messages.Forbidden);

        var validations = await _ApplicationRepo.GetValidationsByApplicationIdAsync(applicationId);
        return validations.Select(ToValidationDto).ToList();
    }

    public async Task<PagedValidationResponseDto> FilterValidationsAsync(
        ValidationFilterDto filter, int currentUserId, string currentUserRole)
    {
        if (currentUserRole == nameof(UserRole.Applicant))
        {
            if (!filter.ApplicationId.HasValue)
                throw new UnauthorizedAccessException(Messages.Forbidden);

            var app = await _ApplicationRepo.GetByIdAsync(filter.ApplicationId.Value)
                ?? throw new KeyNotFoundException(Messages.ApplicationNotFound);

            if (app.ApplicantId != currentUserId)
                throw new UnauthorizedAccessException(Messages.Forbidden);
        }

        var (items, total) = await _ApplicationRepo.FilterValidationsAsync(
            filter.ApplicationId, filter.Result, filter.Page, filter.PageSize);

        return new PagedValidationResponseDto
        {
            Items = items.Select(ToValidationDto).ToList(),
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    private static ValidationResponseDto ToValidationDto(ApplicationValidation v) => new()
    {
        ApplicationValidationId = v.ApplicationValidationId,
        ApplicationId = v.ApplicationId,
        RuleName = v.RuleName ?? string.Empty,
        Result = v.Result ?? string.Empty,
        Message = v.Message ?? string.Empty,
        CheckedDate = v.CheckedDate
    };
}