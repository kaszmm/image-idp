using AutoMapper;
using IdentityServer.Infrastructure;
using IdentityServer.Infrastructure.Repositories;
using IdentityServer.Models;
using IdentityServer.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Serilog;

namespace IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();

        builder.Services.AddIdentityServer(options =>
            {
                // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddProfileService<UserProfileService>(); // this helps idp to get required claims from user when generating access token
            // .AddTestUsers(TestUsers.Users); Implemented custom UserStore, so this is not required now

        // this setting is required for oidc flow redirection from idp to client
        // if we added reverse proxy for client this setting will ensure that new domain is what the redirection will happen on
        // still confused? (ask to chat gpt, or remove and check yourself u dumb ass!)
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                                       ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.Services.AddScoped(
            typeof(IMongoRepository<>), typeof(MongoRepository<>));

        builder.Services.AddScoped<IUserStoreRepository, UserStoreRepository>();

        builder.Services.Configure<MongoDataBaseSettings>(
            builder.Configuration.GetSection("MongoDataBaseSettings"));

        // TODO: investigate why added this code
        builder.Services.AddSingleton<IDatabaseSettings>(sp =>
            sp.GetRequiredService<IOptions<MongoDataBaseSettings>>().Value);

        builder.Services.AddSingleton<IMongoClient>(sp =>
            new MongoClient(sp.GetRequiredService<IDatabaseSettings>().ConnectionString));

        builder.Services.AddScoped<IUserStoreService, UserStoreService>();

        RegisterMongoClassMaps();
        ConfigureAutoMapper(builder.Services);
        
        return builder.Build();
    }

    private static void RegisterMongoClassMaps()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
        {
            BsonClassMap.RegisterClassMap<BaseEntity>(map =>
            {
                map.AutoMap();
                map.MapIdField(x => x.Id);
                map.MapIdProperty(x => x.Id);
            });
        }
    }

    private static void ConfigureAutoMapper(IServiceCollection service)
    {
        var configurations =
            new MapperConfiguration(
                config => config.AddMaps(typeof(IDatabaseRepository).Assembly));
        service.AddScoped(_ => configurations.CreateMapper());
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // without this Oidc flow doesnt work
        app.UseForwardedHeaders(new ForwardedHeadersOptions()
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto
        });

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();
            
        app.UseIdentityServer(); // UseAuthentication call is included inside this call, so its not requited to add UseAuthentication() in pipeline

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
