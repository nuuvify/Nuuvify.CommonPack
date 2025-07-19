using System;
using System.Globalization;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.Security.Helpers;
using Nuuvify.CommonPack.Security.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nuuvify.CommonPack.Security.JwtOpenId
{
    public static class UserOpenIdSecuritySetup
    {

        /// <summary>
        /// Esse metodo apenas injeta <br/>
        /// HttpContextAccessor <br/>
        /// ControllerOpenIdAuthorizationHandler <br/>
        /// UserAuthenticated <br/>
        /// AddRolesClaimsTransformation
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddOpenIdSecuritySetup(this IServiceCollection services, IConfiguration configuration)
        {

            if (services is null)
                throw new ArgumentNullException(nameof(services), MsgSecurityJwt.ResourceManager.GetString("ServiceNull", CultureInfo.CurrentCulture));

            if (configuration is null)
                throw new ArgumentNullException(MsgSecurityJwt.ResourceManager.GetString("ConfigurationNull", CultureInfo.CurrentCulture));


            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationHandler, ControllerOpenIdAuthorizationHandler>();
            services.TryAddScoped<IUserAuthenticated, UserAuthenticated>();
            services.AddScoped<IClaimsTransformation, AddRolesClaimsTransformation>();



        }

    }
}
