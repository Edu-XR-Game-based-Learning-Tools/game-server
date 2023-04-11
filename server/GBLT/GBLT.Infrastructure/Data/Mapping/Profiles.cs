using AutoMapper;
using Core.Entity;

namespace Infrastructure
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<TUser, TIdentityUser>().ConstructUsing(u => new TIdentityUser { UserName = u.UserName, Email = u.Email }).ForMember(au => au.Id, opt => opt.Ignore());
            CreateMap<TIdentityUser, TUser>().ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email)).
                                       ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.PasswordHash));
        }
    }
}