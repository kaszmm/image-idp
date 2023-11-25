using System.IdentityModel.Tokens.Jwt;
using Authorization.Policy;
using ImageGallery.API.Authorization;
using ImageGallery.API.DbContexts;
using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(configure => configure.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddDbContext<GalleryContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:ImageGalleryDBConnectionString"]);
});

// register the repository
builder.Services.AddScoped<IGalleryRepository, GalleryRepository>();
builder.Services.AddScoped<IAuthorizationHandler, MustOwnImageHandler>();

// NOTE: This is require is want to use ' IHttpContextAccessor'
builder.Services.AddHttpContextAccessor();

// register AutoMapper-related services
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// clear the default claim mappings, same we did in client project
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// this service is used by image gallery api to validate the access token, when client tries to
// access the api with bearer token, this will validate the token is whether correct or not
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    
    // USE THIS BLOCK IF access token type is JWT
    // .AddJwtBearer(options =>
    // {
    //     options.Authority = "https://fakeidp.qapitacorp.local";
    //     options.Audience = "imageGalleryApi";
    //     options.TokenValidationParameters = new TokenValidationParameters()
    //     {
    //         // providing same claim type mappings we provided in client project
    //         NameClaimType = "given_name",
    //         RoleClaimType = "role",
    //         ValidTypes = new[] { "at+jwt" }
    //     };
    // })
    
    // USE THIS BLOCK if access token type is Reference token
    // this endpoint will request for access token to idp's introspective endpoint and gets the access token
    .AddOAuth2Introspection(options =>
    {
        options.Authority = "https://fakeidp.qapitacorp.local";
        options.ClientId = "imageGalleryApi";
        options.ClientSecret = "apiSecret"; 
        options.NameClaimType = "given_name";
        options.RoleClaimType = "role";
    });

builder.Services.AddAuthorization(options =>
{
    Console.WriteLine("adding the auth policy");
    // options.AddPolicy("IndianPaidUserCanAddImage", AuthorizationPoliciesBuilder.CanAddImage());
    options.AddPolicy("PaidUserCanRead", policyBuilder =>
    {
        policyBuilder.RequireClaim("scope", "imageGalleryApi.read");
    });
    
    options.AddPolicy("PaidUserCanWrite", policyBuilder =>
    {
        policyBuilder.RequireClaim("scope", "imageGalleryApi.write");
    });

    options.AddPolicy("MustOwnImage",
        policyBuilder =>
        {
            policyBuilder.RequireAuthenticatedUser()
                .AddRequirements(new MustOwnImageRequirement());
        });
    Console.WriteLine("completed adding the auth policy");
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
