using AutoMapper;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.NotificationsDtos;

namespace GrantTrack.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entity to DTO
        CreateMap<Notification, NotificationsReadDto>();

        // DTO to Entity
        CreateMap<NotificationsCreateDto, Notification>();
    }
}