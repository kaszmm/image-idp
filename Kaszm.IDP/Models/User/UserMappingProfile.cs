using AutoMapper;

namespace IdentityServer.Models;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserClaim, UserClaimDto>().ReverseMap();
        CreateMap<UserLogin, UserLoginDto>().ReverseMap();
    }
}