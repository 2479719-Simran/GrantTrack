namespace GrantTrack.AutoMappers;
using AutoMapper;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.EligibilityRulesDtos;
using GrantTrack.Dto.RequiredDocumentsDtos;

public class RequiredDocumentProfile : Profile
{   
    public RequiredDocumentProfile()
    {
        CreateMap<RequiredDocument, CreateRequiredDocumentResponseDto>();
        CreateMap<RequiredDocument, GetRequiredDocumentResponseDto>();
        CreateMap<RequiredDocument, UpdateRequiredDocumentResponseDto>();
        CreateMap<RequiredDocument, DeleteRequiredDocumentResponseDto>();
    }
}