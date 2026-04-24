using System;
using AutoMapper;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.EligibilityRulesDtos;

namespace GrantTrack.AutoMappers;

public class EligibilityRuleProfile : Profile
{
    public EligibilityRuleProfile()
    {
        CreateMap<EligibilityRule , CreateEligibilityRuleResponseDto>(); 
        CreateMap<EligibilityRule , UpdateEligibilityRuleResponseDto>(); 
        CreateMap<EligibilityRule , DeleteEligibilityRuleResponseDto>(); 
        CreateMap<EligibilityRule , GetEligibilityRuleResponseDto>(); 
    }
}
