using AutoMapper;
using IdentityServer.Infrastructure.Repositories;
using IdentityServer.Models;

namespace IdentityServer.Services;

public interface IUserStoreService
{
    Task<Guid> CreatUser(UserDto user);
}

public class UserStoreService : IUserStoreService
{
    private readonly IUserStoreRepository _userStoreRepository;
    private readonly IMapper _mapper;

    public UserStoreService(IUserStoreRepository userStoreRepository, IMapper mapper)
    {
        _userStoreRepository = userStoreRepository ?? throw new ArgumentNullException(nameof(userStoreRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Guid> CreatUser(UserDto user)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new ArgumentException("value is required", nameof(user.Email));
        }

        if (string.IsNullOrWhiteSpace(user.Password))
        {
            throw new ArgumentException("value is required", nameof(user.Password));
        }

        if (string.IsNullOrWhiteSpace(user.FirstName))
        {
            throw new ArgumentException("value is required", nameof(user.FirstName));
        }

        var existingUserWithEmail = await _userStoreRepository.GetAsync(u => u.Email == user.Email);
        if (existingUserWithEmail != null)
        {
            throw new ArgumentException("User with same email already exist");
        }

        var domainUser = _mapper.Map<User>(user);
        await _userStoreRepository.CreateAsync(domainUser);
        return domainUser.Id;
    }
}