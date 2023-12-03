using IdentityServer.Models;
using IdentityServer.Services;

namespace IdentityServer;

public class SeedData
{
    public static async Task SeedUsers(IUserStoreService userStoreService)
    {
        Guid createdUserId;
        createdUserId = await userStoreService.CreatUser(
            new UserDto("Kasim", "Nala",
                "password", "kasim@mail.com",
                "admin", new[] { new UserClaimDto("role", "admin") }));

        Console.WriteLine($"Created user with Id :{createdUserId}");

        createdUserId = await userStoreService.CreatUser(
            new UserDto("Naama", "Dhundhiya",
                "password", "naama@mail.com",
                "employee", new[] { new UserClaimDto("role", "employee") }));

        Console.WriteLine($"Created user with Id :{createdUserId}");
    }
}