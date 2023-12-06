using IdentityModel;
using IdentityServer.Models;
using IdentityServer.Services;

namespace IdentityServer;

public class SeedData
{
    public static async Task SeedUsers(IUserStoreService userStoreService)
    {
        var createdUser = await userStoreService.CreatUserAsync(
            new UserDto("", "Kasim", "Nala",
                "password", "kasim@mail.com",
                true, default,default,"admin", new[]
                {
                    new UserClaimDto("role", "admin"),
                    new UserClaimDto("country", "ind"),
                    new UserClaimDto(JwtClaimTypes.Name, "Kasim Nala"),
                    new UserClaimDto(JwtClaimTypes.GivenName, "Kasim"),
                    new UserClaimDto(JwtClaimTypes.FamilyName, "Nalawala"),
                    new UserClaimDto(JwtClaimTypes.Email, "kasim@mail.com"),
                }));

        Console.WriteLine($"Created user with Id :{createdUser.Id}");

        createdUser = await userStoreService.CreatUserAsync(
            new UserDto("", "Naama", "Dhundhiya",
                "password", "naama@mail.com", true,
                default,default,
                "employee", new[]
                {
                    new UserClaimDto("role", "employee"),
                    new UserClaimDto("country", "uae"),
                    new UserClaimDto(JwtClaimTypes.Name, "Naama Dhundhiya"),
                    new UserClaimDto(JwtClaimTypes.GivenName, "Naama"),
                    new UserClaimDto(JwtClaimTypes.FamilyName, "Dhundhiya"),
                    new UserClaimDto(JwtClaimTypes.Email, "naama@mail.com"),
                }));

        Console.WriteLine($"Created user with Id :{createdUser.Id}");
    }
}