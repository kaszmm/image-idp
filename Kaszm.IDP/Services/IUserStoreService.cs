using AutoMapper;
using IdentityServer.Infrastructure.Repositories;
using IdentityServer.Models;

namespace IdentityServer.Services;

public interface IUserStoreService
{
    Task<Guid> CreatUser(UserDto user);

    Task<bool> ValidateCredentialsAsync(string userEmail, string password);

    Task<UserDto> GetUserByUserEmail(string userEmail);
    
    Task<IEnumerable<UserClaimDto>> GetUserClaims(Guid userId);

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

        // updates the userName with the email the user passed
        user = user with
        {
            UserName = user.Email
        };
        
        var domainUser = _mapper.Map<User>(user);
        await _userStoreRepository.CreateAsync(domainUser);
        return domainUser.Id;
    }

    public async Task<bool> ValidateCredentialsAsync(string userEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            throw new ArgumentException("value is required", nameof(userEmail));
        }
        
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("value is required", nameof(password));
        }

        var user =
            await _userStoreRepository.GetAsync(u => u.Email == userEmail && 
                                                     u.Password == password);
        return user != null;
    }

    public async Task<UserDto> GetUserByUserEmail(string userEmail)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            throw new ArgumentException("value is required", nameof(userEmail));
        }

        var user =
            await _userStoreRepository.GetAsync(u => u.Email == userEmail);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserClaimDto>> GetUserClaims(Guid userId)
    {
        var user =
            await _userStoreRepository.GetAsync(u => u.Id == userId);

        if (user is null)
        {
            throw new ArgumentException("user doesn't exist");
        }

        return _mapper.Map<IEnumerable<UserClaimDto>>(user.UserClaims);
    }
}