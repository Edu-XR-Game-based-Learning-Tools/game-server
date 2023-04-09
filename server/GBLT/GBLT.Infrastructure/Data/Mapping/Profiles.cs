using AutoMapper;
using LMS.Server.Core.Domain;
using LMS.Server.Core.Dto;
using LMS.Server.Infrastructure.Identity;

namespace LMS.Server.Infrastructure.Data
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<TUser, TIdentityUser>().ConstructUsing(u => new TIdentityUser { UserName = u.UserName, Email = u.Email }).ForMember(au => au.Id, opt => opt.Ignore());
            CreateMap<TIdentityUser, TUser>().ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email)).
                                       ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.PasswordHash));

            CreateMap<TUser, UserDto>();
            CreateMap<UserDto, TUser>();

            CreateMap<TCourse, CourseDto>();
            CreateMap<TLesson, LessonDto>();
            CreateMap<TTest, TestDto>();
        }
    }
}