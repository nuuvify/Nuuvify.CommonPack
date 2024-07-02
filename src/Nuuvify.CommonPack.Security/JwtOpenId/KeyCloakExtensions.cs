using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Nuuvify.CommonPack.Security.JwtOpenId
{
    public static class KeyCloakExtensions
    {

        private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                // TODO: Use your User Agent library of choice here. 
                //if (/* UserAgent doesn’t support new behavior */)
                {
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }

        public static IServiceCollection AddKeyCloak(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext => KeyCloakExtensions.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext => KeyCloakExtensions.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

            })
           .AddCookie(options =>
           {
               options.LoginPath = new PathString("/perfil/entrar");
               options.LogoutPath = new PathString("/perfil/sair");
               options.AccessDeniedPath = new PathString("/perfil/acesso-negado");
           })
           .AddOpenIdConnect(options =>
           {
               configuration.Bind("oidc", options);

               options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

               options.Events = new OpenIdConnectEvents
               {
                   OnAuthenticationFailed = x =>
                   {
                       return Task.CompletedTask;
                   },
                   OnAuthorizationCodeReceived = x =>
                   {
                       return Task.CompletedTask;
                   },
                   OnMessageReceived = x =>
                   {
                       return Task.CompletedTask;
                   },
                   OnRedirectToIdentityProviderForSignOut = x =>
                   {
                       return Task.CompletedTask;
                   },
                   OnSignedOutCallbackRedirect = x =>
                   {
                       return Task.CompletedTask;
                   },
                   OnRemoteSignOut = x =>
                   {
                       return Task.CompletedTask;
                   },
                   OnTokenResponseReceived = tokenResponseReceivedContext =>
                   {
                        //BsonDocument doc = ConvertToBsonDocument(tokenResponseReceivedContext);
                        //this.ApplicationServices.GetRequiredService<LogService>().Log(doc);
                        return Task.CompletedTask;
                   },
                   OnUserInformationReceived = userInformationReceivedContext =>
                   {
                        //this.ApplicationServices.GetRequiredService<ProfileService>().OnUserInformationReceived(userInformationReceivedContext);
                        return Task.CompletedTask;
                   },
                   OnAccessDenied = x =>
                   {
                       return Task.CompletedTask;
                   },
                   OnRemoteFailure = context =>
                   {
                       context.Response.Redirect("/");
                       context.HandleResponse();
                       return Task.CompletedTask;
                   },
                   OnTicketReceived = x =>
                   {
                       return Task.CompletedTask;
                   },

                   OnRedirectToIdentityProvider = context =>
                   {
                       context.ProtocolMessage.RedirectUri = $"{configuration.GetValue<string>("applicationUrl")}/signin-oidc";

                       return Task.CompletedTask;
                   },

                   OnTokenValidated = ConvertKeycloakRolesInAspNetRoles,
               };



           });


            return services;
        }

        private static Task ConvertKeycloakRolesInAspNetRoles(Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext context)
        {
            var claim = context.SecurityToken.Claims.SingleOrDefault(it => it.Type == "resource_access" && it.ValueType == "JSON");
            if (claim != null)
            {
                JObject value = JsonConvert.DeserializeObject<JObject>(claim.Value);
                string audience = context.SecurityToken.Audiences.Single();
                var prop = value[audience];
                var roles = prop?["roles"];
                if (roles != null)
                {
                    var identity = (ClaimsIdentity)context.Principal.Identity;
                    identity.AddClaims(
                        roles.Select(it => new Claim(ClaimTypes.Role, it.Value<string>())).ToArray()
                    );
                }
            }
            return Task.CompletedTask;
        }


        public static void AddProfilePolicy(this AuthorizationOptions options)
        {
            options.AddPolicy("profile", new AuthorizationPolicyBuilder(OpenIdConnectDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .RequireClaim(ClaimTypes.Email)
                        .RequireClaim("email_verified", "true")
                        .Build());
        }

        public static void AddRequiredRolePolicy(this AuthorizationOptions options, string policyName, string role)
        {
            options.AddPolicy(policyName, new AuthorizationPolicyBuilder(OpenIdConnectDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .RequireClaim("email_verified", "true")
                    .RequireClaim(ClaimTypes.Email)
                    .RequireClaim(ClaimTypes.Role, role)
                    .Build());
        }

    }
}