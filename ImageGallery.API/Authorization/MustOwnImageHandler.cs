using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ImageGallery.API.Authorization;

public class MustOwnImageHandler : AuthorizationHandler<MustOwnImageRequirement>
{
    private readonly IGalleryRepository _galleryRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MustOwnImageHandler(IGalleryRepository galleryRepository, IHttpContextAccessor httpContextAccessor)
    {
        _galleryRepository = galleryRepository ?? throw new ArgumentNullException(nameof(galleryRepository));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }
    
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        MustOwnImageRequirement requirement)
    {
        var imageId = _httpContextAccessor.HttpContext?.GetRouteValue("id")?.ToString();
        if (!Guid.TryParse(imageId, out var imageGuid) && !string.IsNullOrWhiteSpace(imageId))
        {
            Console.WriteLine("Failed not having a image validation");
            context.Fail(new AuthorizationFailureReason(this, "no image found"));
            return;
        }

        var ownerId = context.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            Console.WriteLine("Failed for owner being null");
            context.Fail(new AuthorizationFailureReason(this, "no owner found"));
            return;
        }

        var isImageOwner = await _galleryRepository.IsImageOwnerAsync(imageGuid, ownerId);
        if (!isImageOwner)
        {
            Console.WriteLine("Failed for not being owner of image");
            context.Fail(new AuthorizationFailureReason(this, "image is not owned by owner"));
            return;
        }

        context.Succeed(requirement);
        Console.WriteLine("requirement handler succeeded");
    }
}