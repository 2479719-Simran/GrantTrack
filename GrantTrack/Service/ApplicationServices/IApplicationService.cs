using System;
using GrantTrack.Dto.ApplicationDtos;

namespace GrantTrack.Service.ApplicationServices;

public interface IApplicationService
{
    Task<ApplicationResponseDto> CreateDraftAsync(CreateApplicationDto dto, int applicantId);
    Task<ApplicationResponseDto> SubmitAsync(int applicationId, int applicantId);
}
