using System;
using System.Linq;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>().ForMember(
                    dest => dest.PhotoUrl,
                    opt => opt.MapFrom(u => u.Photos.FirstOrDefault(p => p.IsMain).Url)
                )
                .ForMember(
                    dest => dest.Age,
                    opt => opt.MapFrom(u => u.DateOfBirth.CalculateAge())
                );
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDTO, AppUser>();
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderPhotoUrl,
                    opt => opt
                        .MapFrom(m => m.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(dest => dest.RecipientPhotoUrl,
                    opt => opt
                        .MapFrom(m => m.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));
            
            // CreateMap<DateTime, DateTime>()
            //     .ConvertUsing(d => DateTime.SpecifyKind(d,DateTimeKind.Utc));
        }
    }
}