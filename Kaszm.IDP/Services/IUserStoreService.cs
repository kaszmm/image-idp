using AutoMapper;
using IdentityServer.Infrastructure.Repositories;
using IdentityServer.Models;

namespace IdentityServer.Services;

public interface IUserStoreService
{
    Task<Guid> CreatUserAsync(UserDto user);

    Task<bool> ValidateCredentialsAsync(string userName, string password);

    Task<UserDto> GetUserByUserNameAsync(string userName);

    Task<IEnumerable<UserClaimDto>> GetUserClaimsAsync(string userId);

    Task<bool> IsActiveUserAsync(string userId);
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

    public async Task<Guid> CreatUserAsync(UserDto user)
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

        // updates the userName with the email the user passed
        user = user with
        {
            UserName = user.Email
        };

        var domainUser = _mapper.Map<User>(user);
        await _userStoreRepository.CreateAsync(domainUser);
        return domainUser.Id;
    }

    public async Task<bool> ValidateCredentialsAsync(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("value is required", nameof(userName));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("value is required", nameof(password));
        }

        var user =
            await _userStoreRepository.GetAsync(filter =>
                filter.UserName == userName && filter.Password == password &&
                filter.IsActive);
        return user != null;
    }

    public async Task<UserDto> GetUserByUserNameAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("value is required", nameof(userName));
        }

        var user =
            await _userStoreRepository.GetAsync(u => u.UserName == userName);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserClaimDto>> GetUserClaimsAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var guidUserId))
        {
            return new List<UserClaimDto>();
        }
        
        var user =
            await _userStoreRepository.GetUserAsync(guidUserId);

        if (user is null)
        {
            throw new ArgumentException("user doesn't exist");
        }

        return _mapper.Map<IEnumerable<UserClaimDto>>(user.UserClaims);
    }

    public async Task<bool> IsActiveUserAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var guidUserId))
        {
            return false;
        }
        
        var user =
            await _userStoreRepository.GetUserAsync(guidUserId);
        
        return user != null;
    }
}