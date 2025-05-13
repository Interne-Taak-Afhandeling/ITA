using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.PortableExecutable;

namespace InterneTaakAfhandeling.Web.Server.Authentication
{
    public static class AuthenticationExtensions
    {
        private const string SignOutCallback = "/signout-callback-oidc";
        private const string CookieSchemeName = "cookieScheme";
        private const string ChallengeSchemeName = "challengeScheme";

        public static void AddAuth(this IServiceCollection services, Action<AuthOptions> setOptions)
        {
            var authOptions = new AuthOptions();
            setOptions(authOptions);

            var objectregisterMedewerkerIdClaimType = authOptions.ObjectregisterMedewerkerIdClaimType;
            var emailClaimType = string.IsNullOrWhiteSpace(authOptions.EmailClaimType) ? JwtClaimTypes.Email : authOptions.EmailClaimType;
            var nameClaimType = string.IsNullOrWhiteSpace(authOptions.NameClaimType) ? JwtClaimTypes.Name : authOptions.NameClaimType;
            var roleClaimType = string.IsNullOrWhiteSpace(authOptions.RoleClaimType) ? JwtClaimTypes.Roles : authOptions.RoleClaimType;
          
            services.AddHttpContextAccessor();

            services.AddScoped<ITAUser>(s =>
            {
                var user = s.GetRequiredService<IHttpContextAccessor>().HttpContext?.User;
                var isLoggedIn = user?.Identity?.IsAuthenticated ?? false;
                var objectregisterMedewerkerId = string.IsNullOrWhiteSpace(objectregisterMedewerkerIdClaimType) ? string.Empty : user?.FindFirst(objectregisterMedewerkerIdClaimType)?.Value ?? string.Empty;
                var name = user?.FindFirst(nameClaimType)?.Value ?? string.Empty;
                var email = user?.FindFirst(emailClaimType)?.Value ?? string.Empty;
                var roles = user?.FindAll(roleClaimType).Select(x=> x.Value).ToArray() ?? [];
                var hasITASystemAccess = roles.Contains(authOptions.ITASystemAccessRole);

                if(!string.IsNullOrWhiteSpace(objectregisterMedewerkerIdClaimType) && string.IsNullOrWhiteSpace(objectregisterMedewerkerId))
                {
                    throw new Exception($"Verwachtewaarde voor de {authOptions.ObjectregisterMedewerkerIdClaimType} ontbreekt. Neem contact op met de beheerder.");
                }


                return new ITAUser {ObjectregisterMedewerkerId= objectregisterMedewerkerId , IsLoggedIn = isLoggedIn, Name = name, Email = email, Roles = roles, HasITASystemAccess = hasITASystemAccess };
            });

            var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieSchemeName;
                if (!string.IsNullOrWhiteSpace(authOptions.Authority))
                {
                    options.DefaultChallengeScheme = ChallengeSchemeName;
                }
            }).AddCookie(CookieSchemeName, options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true; 
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true; 
                options.Events.OnRedirectToAccessDenied = (ctx) =>
                { 
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToLogin = HandleLoggedOut;
            });

            if (!string.IsNullOrWhiteSpace(authOptions.Authority))
            {
                authBuilder.AddOpenIdConnect(ChallengeSchemeName, options =>
                {
                    options.NonceCookie.HttpOnly = true;
                    options.NonceCookie.IsEssential = true;
                    options.NonceCookie.SameSite = SameSiteMode.None;
                    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.CorrelationCookie.HttpOnly = true;
                    options.CorrelationCookie.IsEssential = true;
                    options.CorrelationCookie.SameSite = SameSiteMode.None;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

                    options.Authority = authOptions.Authority;
                    options.ClientId = authOptions.ClientId;
                    options.ClientSecret = authOptions.ClientSecret;
                    options.SignedOutRedirectUri = SignOutCallback;
                    options.ResponseType = OidcConstants.ResponseTypes.Code;
                    options.UsePkce = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.Scope.Clear();
                    options.Scope.Add(OidcConstants.StandardScopes.OpenId);
                    options.Scope.Add(OidcConstants.StandardScopes.Profile); 
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        RoleClaimType = roleClaimType,
                    };


                    options.Events.OnRemoteFailure = RedirectToRoot;
                    options.Events.OnSignedOutCallbackRedirect = RedirectToRoot;
                    options.Events.OnRedirectToIdentityProvider = (ctx) =>
                    {
                        if (!ctx.Request.IsBrowserNavigation())
                        {
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            ctx.Response.Headers.Location = ctx.ProtocolMessage.CreateAuthenticationRequestUrl();
                            ctx.HandleResponse();
                        }
                        return Task.CompletedTask;
                    };
                });
            }
            services.AddAuthorizationBuilder()
                .AddPolicy(ITAPolicy.Name, policy => policy.RequireRole(authOptions.ITASystemAccessRole))
                .AddFallbackPolicy("LoggedIn", policy => policy.RequireAuthenticatedUser());
            services.AddDistributedMemoryCache();
            services.AddOpenIdConnectAccessTokenManagement();
        }

        public static IEndpointRouteBuilder MapITAAuthEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("api/logoff", LogoffAsync).AllowAnonymous();
            endpoints.MapGet("api/me", (ITAUser user) => user).AllowAnonymous();
            endpoints.MapGet("api/challenge", ChallengeAsync).AllowAnonymous();

            return endpoints;
        }

        private static async Task LogoffAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieSchemeName);
            await httpContext.SignOutAsync(ChallengeSchemeName);
        }

        private static Task RedirectToRoot<TOptions>(HandleRequestContext<TOptions> context) where TOptions : AuthenticationSchemeOptions
        {
            context.Response.Redirect("/");
            context.HandleResponse();

            return Task.CompletedTask;
        }

        private static Task HandleLoggedOut<TOptions>(RedirectContext<TOptions> ctx) where TOptions : AuthenticationSchemeOptions
        {
            if (!ctx.Request.IsBrowserNavigation())
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Response.Headers.Location = ctx.RedirectUri;
            }
            return Task.CompletedTask;
        }

        private static Task ChallengeAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var returnPath = GetRelativeReturnUrl(request);

            if (httpContext.User.Identity?.IsAuthenticated ?? false)
            {
                httpContext.Response.Redirect(returnPath);
                return Task.CompletedTask;
            }

            return httpContext.ChallengeAsync(new AuthenticationProperties
            {
                RedirectUri = returnPath,
            });
        }
         
        private static string GetRelativeReturnUrl(HttpRequest request)
        {
            var returnUrl = request.Query["returnUrl"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(returnUrl) || new Uri(returnUrl, UriKind.RelativeOrAbsolute).IsAbsoluteUri) return "/";
            return $"/{returnUrl.AsSpan().TrimStart('/')}";
        }

        private static bool IsBrowserNavigation(this HttpRequest request) => request.Headers.TryGetValue("Sec-Fetch-Dest", out var secFetchDest) && secFetchDest == "document";
    }
}
