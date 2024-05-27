using API.DTOs.Requests.User;
using API.DTOs.Responses.User;
using API.Entities;
using AutoMapper;

namespace API.Profiles
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, UserDto>().ReverseMap();
            CreateMap<User_LoginReq, AppUser>();
            CreateMap<User_RegisterReq, AppUser>();
        }
    }
}
