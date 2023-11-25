using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Authorization.Policy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(configure => 
        configure.JsonSerializerOptions.PropertyNamingPolicy = null);

// microsoft for supporting backwards compatibility with Ws security, used to map claims with old names,
// like "http://schemas.microsoft.com/identity/claims/identityprovider" instead of "sub", if we call clear on this mapper then it
// will map claims with what idp provides us
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// used to register the accessTokenManagement services in DI
builder.Services.AddAccessTokenManagement();

// create an HttpClient used for accessing the API
builder.Services.AddHttpClient("APIClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ImageGalleryAPIRoot"]);
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
}).AddUserAccessTokenHandler(); // this service adds the access token in authorization header on each http request,
                                // also in case when access token is expired,
                                // it automatically gets new token using refresh token


// create an HttpClient used for accessing the IDP
builder.Services.AddHttpClient("IDPClient",
    client =>
    {
        client.BaseAddress = new Uri("https://fakeidp.qapitacorp.local");
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin();
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
    {
        // default scheme is what application uses the check whether the user is authenticated for each request,
        // we had set that scheme as cookie, means once user successfully logged in we will save the user identity in cookie
        // which by default will be send on each request call in authorization header of each request
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // challenge scheme is for when user tries to access protected resource, and are not logged in
        // then this scheme application will challenge the user to get authenticated (in our case by challenge is oidc)
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

        // NOTE: this is set when calling AddOpenIdConnect and AddCookie middleware, so need to explicitly add here
        // options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.AccessDeniedPath = "/Authentication/AccessDenied";
    })
    .AddOpenIdConnect(options =>
    {
        // this tells middleware using which scheme to create 'id provider'
        // this 'id provider' is later used by defaultScheme for subsequent requests
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // It tells the middleware where to redirect the user for authentication and where to retrieve the OIDC discovery document.
        options.Authority = "https://fakeidp.qapitacorp.local";
        options.ClientId = "imageGalleryClient";
        options.ClientSecret = "secret";

        // It indicates the kind of authentication flow you're using
        options.ResponseType = "code";

        // It indicates after authenticating from idp, in which format the idp should return the token or code.
        // NOTE: middleware by defaults adds this code, so no need to add here
        // options.ResponseMode = "form_post";

        // NOTE: middleware by defaults adds this code, so no  need to add here
        // options.Scope.Add("openid");
        // options.Scope.Add("profile");
        options.Scope.Add("roles");
        options.Scope.Add("imageGalleryApi.read");
        options.Scope.Add("imageGalleryApi.write");
        options.Scope.Add("country");
        
        // for refresh token
        options.Scope.Add("offline_access");


        // once user get authenticate in idp, the idp will redirect user back to this uri
        // here the middleware will intercept the request and gets the code and use it to exchange it with access token
        // this all is done by back channel flow (server to server communication)
        // NOTE: default value for callback path is "signin-oidc" so no need to add here
        // options.CallbackPath = new PathString("signin-oidc");
        // options.SignedOutCallbackPath = new PathString("signout-callback-oidc");
        
        options.SaveTokens = true;

        // by default user claims are not added in access token, for reasons like making token too large to process in 
        // old browsers where the url limit might exceed, and also keep token light
        // Behaviour: enabling this will allow the middleware to call /userinfo(protected and can be accessed with access token) endpoint in IDP, 
        // to get authenticated user claims, this happens through back channel
        options.GetClaimsFromUserInfoEndpoint = true;
        
        // this will allow the "aud" claim to be added in User.Claims,
        // this "removes" filter that actually doesnt allow "aud" to be added in claims
        options.ClaimActions.Remove("aud");
        
        // this will delete the claim "idp" from User.Claims
        // this "delete" the claim "idp" itself
        options.ClaimActions.DeleteClaim("idp");
        
        // in order to map the incoming claims(from token) to microsoft's identity user claim, we need to explicitly map the claims
        options.ClaimActions.MapJsonKey("role", "role");
        options.ClaimActions.MapUniqueJsonKey("country", "country");
        
        // although adding and mapping role claims, we still be not able to set the roleClaim for user,
        // cause by default the .net sets Role claim to be : "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" and not "role"
        // so we need to manually tell .net to map RoleClaim with "role", same goes for Name claim as well
        
        // TL;DR: TokenValidationParameters dictate which claims in the token should be treated as the user's name and role(s), respectively
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "given_name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PaidUserCanAddImage", AuthorizationPoliciesBuilder.CanAddImage());
});

// this is the additional configuration we need to add, without this, the redirect uri generated by .net will be http and https,
// cause originally the program is running on http, we are reverse proxing it to https using nginx
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto |
                               ForwardedHeaders.XForwardedHost;

    // this basically tells the asp.net to not limit or restrict to certain known networks and proxies,
    // and trust all forward headers and proxies
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

// this should be added before any other pipeline
// NOTE: without this the forward header configuration will not be used in middleware
app.UseForwardedHeaders();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


// this allow that all the different domain request dont get blocked by cors, like accessing api domain from client 
app.UseCors("AllowAllOrigins");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Gallery}/{action=Index}/{id?}");

app.Run();
