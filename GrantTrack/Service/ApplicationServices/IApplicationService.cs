using System;
using GrantTrack.Dto.ApplicationDtos;
using GrantTrack.Dto.DisbursementDtos;

namespace GrantTrack.Service.ApplicationServices;

public interface IApplicationService
{
    Task<ApplicationResponseDto> CreateDraftAsync(CreateApplicationDto dto, int applicantId);
    Task<ApplicationResponseDto> SubmitAsync(int applicationId, int applicantId);
    Task<ApplicationResponseDto> GetByIdAsync(int applicationId, int applicantId);

    Task<List<ValidationResponseDto>> GetValidationsAsync(int applicationId, int currentUserId, string currentUserRole);
    Task<PagedValidationResponseDto> FilterValidationsAsync(
        ValidationFilterDto filter, int currentUserId, string currentUserRole);
}