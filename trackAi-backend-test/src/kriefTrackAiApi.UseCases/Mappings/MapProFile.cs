using AutoMapper;
using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kriefTrackAiApi.UseCases.Mappings;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.CompanyIds, opt => opt.MapFrom(src => src.CompanyIds ?? new List<Guid>()));

        CreateMap<UserDto, User>()
            .ForMember(dest => dest.CompanyIds, opt => opt.MapFrom(src => src.CompanyIds ?? new List<Guid>()));

        CreateMap<Task<User>, Task<UserDto>>()
            .ConvertUsing<TaskTypeConverter<User, UserDto>>();

        CreateMap<Task<List<User>>, Task<List<UserDto>>>()
            .ConvertUsing<TaskTypeConverter<List<User>, List<UserDto>>>();

        CreateMap<Task<IEnumerable<User>>, Task<List<UserDto>>>()
            .ConvertUsing<TaskTypeConverter<IEnumerable<User>, List<UserDto>>>();

        CreateMap<Company, CompanyDto>().ReverseMap();
        CreateMap<Task<Company>, Task<CompanyDto>>().ConvertUsing<TaskTypeConverter<Company, CompanyDto>>();

        CreateMap<UserPhoneEntryDto, UserPhoneEntry>();
        CreateMap<UserPhoneEntry, UserPhoneEntryDto>();

        CreateMap<Sms, SmsDto>();
        CreateMap<SmsDto, Sms>();
        CreateMap<LoginRequestDto, User>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));
        CreateMap<Company, CompanyWithUsersDto>();

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.CompanyIds, opt => opt.MapFrom(src => src.CompanyIds ?? new List<Guid>()))
            .ForMember(dest => dest.Reminders, opt => opt.MapFrom(src => src.Reminders ?? Array.Empty<string>()));

        CreateMap<UserDto, User>()
            .ForMember(dest => dest.CompanyIds, opt => opt.MapFrom(src => src.CompanyIds ?? new List<Guid>()))
            .ForMember(dest => dest.Reminders, opt => opt.MapFrom(src => src.Reminders ?? Array.Empty<string>()));

    }
}
