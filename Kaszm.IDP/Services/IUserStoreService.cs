using System.Security.Cryptography;
using AutoMapper;
using IdentityModel;
using IdentityServer.Infrastructure.Repositories;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Services;

public interface IUserStoreService
{
    Task<UserDto> CreatUserAsync(UserDto user);

    Task UpdateUserAsync(UserDto user);

    Task<UserDto> GetUserAsync(Guid userId);

    Task<bool> ValidateCredentialsAsync(string userName, string password);

    Task<UserDto> GetUserByUserNameAsync(string userName);

    Task<IEnumerable<UserClaimDto>> GetUserClaimsAsync(string userId);

    Task<bool> IsActiveUserAsync(string userId);

    Task<bool> VerifySecurityCode(Guid userId, string securityCode);

    Task<UserDto> GetUserByExternalProviderAsync(string provider, string providerIdentityKey);

    Task<UserDto> ProvisionExternalUserAsync(string email, IEnumerable<UserClaimDto> userClaims,
        UserLoginDto userLogin);
}

public class UserStoreService : IUserStoreService
{
    private readonly IUserStoreRepository _userStoreRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IMapper _mapper;

    public UserStoreService(IUserStoreRepository userStoreRepository, IMapper mapper,
        IPasswordHasher<User> passwordHasher)
    {
        _userStoreRepository = userStoreRepository ?? throw new ArgumentNullException(nameof(userStoreRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<UserDto> CreatUserAsync(UserDto user)
    {
        ValidationUtility.NotNullOrWhitespace(user.Email);
        ValidationUtility.NotNullOrWhitespace(user.Password);
        ValidationUtility.NotNullOrWhitespace(user.FirstName);

        var existingUserWithEmail = await _userStoreRepository.GetAsync(u => u.Email == user.Email);
        if (existingUserWithEmail != null)
        {
            throw new ArgumentException("User with same email already exist");
        }

        var domainUser = _mapper.Map<User>(user);

        domainUser = domainUser with
        {
            IsEmailVerified = false,
            UserName = domainUser.Email,
            Password = _passwordHasher.HashPassword(domainUser, user.Password),
            SecurityCode = Convert.ToBase64String(RandomNumberGenerator.GetBytes(128)),
            SecurityCodeExpiration = DateTime.UtcNow.AddDays(1)
        };

        await _userStoreRepository.CreateAsync(domainUser);
        return _mapper.Map<UserDto>(domainUser);
    }

    public async Task UpdateUserAsync(UserDto user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var userExist = await _userStoreRepository.AnyAsync(x => x.Id == user.Id);
        if (!userExist)
        {
            throw new ArgumentException("User doesnt exist");
        }

        await _userStoreRepository.UpdateAsync(_mapper.Map<User>(user));
    }

    public async Task<UserDto> GetUserAsync(Guid userId)
    {
        return _mapper.Map<UserDto>(await _userStoreRepository.GetUserAsync(userId));
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

        var user = await _userStoreRepository.GetAsync(x => x.UserName == userName);

        if (user is null || !user.IsActive)
        {
            return false;
        }

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
            user, user.Password, password);

        return passwordVerificationResult == PasswordVerificationResult.Success;
    }

    public async Task<UserDto> GetUserByUserNameAsync(string userName)
    {
        ValidationUtility.NotNullOrWhitespace(userName);

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

        var isUserActive =
            await _userStoreRepository.AnyAsync(x => x.Id == guidUserId && x.IsActive);

        return isUserActive;
    }

    public async Task<bool> VerifySecurityCode(Guid userId, string securityCode)
    {
        ValidationUtility.NotNullOrWhitespace(securityCode);

        var user = await _userStoreRepository.GetUserAsync(userId);

        if (user is null)
        {
            throw new ArgumentException("Invalid user");
        }

        var isValidSecurityCode = user.SecurityCode == securityCode &&
                                  DateTime.UtcNow <= user.SecurityCodeExpiration;
        return isValidSecurityCode;
    }

    public async Task<UserDto> GetUserByExternalProviderAsync(string provider, string providerIdentityKey)
    {
        ValidationUtility.NotNullOrWhitespace(provider);
        ValidationUtility.NotNullOrWhitespace(providerIdentityKey);

        var user =
            await _userStoreRepository.GetAsync(u =>
                u.IsActive &&
                u.UserLogins.Any(
                    login => login.Provider == provider &&
                             login.ProviderIdentityKey == providerIdentityKey));

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> ProvisionExternalUserAsync(string email, IEnumerable<UserClaimDto> userClaims,
        UserLoginDto userLogin)
    {
        ValidationUtility.NotNullOrWhitespace(email);
        ArgumentNullException.ThrowIfNull(userClaims);
        ArgumentNullException.ThrowIfNull(userLogin);

        // if user already exist in system, just link them
        var user = await _userStoreRepository.GetAsync(u => u.Email == email);
        if (user != null)
        {
            var updatedUserLogin = user.UserLogins.ToList();

            if (updatedUserLogin.TrueForAll(l => l.Provider != userLogin.Provider))
            {
                updatedUserLogin.Add(_mapper.Map<UserLogin>(userLogin));
                user = user with
                {
                    UserLogins = updatedUserLogin
                };
                await _userStoreRepository.UpdateAsync(user);
            }
        }
        else
        {
            // if user doesnt exist in system, provision a new user for them
            
            var userRoleClaim = userClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Role);

            if (userRoleClaim is null)
            {
                throw new InvalidOperationException("user role claim is required");
            }
            
            var givenNameClaim = userClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName);

            if (givenNameClaim is null)
            {
                throw new InvalidOperationException("user given name claim is required");
            }

            var domainUser = new User(email, givenNameClaim.Value,
                "", "",
                email, false,
                Convert.ToBase64String(RandomNumberGenerator.GetBytes(128)),
                DateTime.UtcNow.AddDays(1),
                userRoleClaim.Value, 
                _mapper.Map<IReadOnlyCollection<UserClaim>>(userClaims),
                new[]
                {
                    _mapper.Map<UserLogin>(userLogin)
                });

            await _userStoreRepository.CreateAsync(domainUser);

            user = domainUser;
        }

        return _mapper.Map<UserDto>(user);
    }
}