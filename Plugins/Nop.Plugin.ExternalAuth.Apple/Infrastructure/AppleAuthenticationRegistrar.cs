using System.Threading.Tasks;
using AspNet.Security.OAuth.Apple;
using Microsoft.AspNetCore.Authentication;
using Nop.Services.Authentication.External;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using System.Linq;

namespace Nop.Plugin.ExternalAuth.Apple.Infrastructure
{
    public class AppleAuthenticationRegistrar : IExternalAuthenticationRegistrar
    {
        public void Configure(AuthenticationBuilder builder)
        {

            builder.AddApple(AppleAuthenticationDefaults.AuthenticationScheme, options =>
            {
                var settings = EngineContext.Current.Resolve<AppleExternalAuthSettings>();


                if (settings == null || string.IsNullOrEmpty(settings.ClientId))
                    return; // plugin not configured

                // Generate Apple JWT client secret
                var clientSecret = AppleClientSecret.Generate   (
                    settings.TeamId,
                    settings.ClientId,
                    settings.KeyId,
                   settings.PrivateKey
                );
                options.ClientId = settings.ClientId;
                options.ClientSecret = clientSecret;
                options.SaveTokens = true;
                options.CallbackPath = "/apple/callback";
                options.Events = new AppleAuthenticationEvents
                {
                    OnRemoteFailure = context =>
                    {
                        // Handles cancel or other errors
                        context.HandleResponse(); // prevent default error page

                        // Redirect to your login callback
                        context.Response.Redirect("/appleauthentication/logincallback");
                        return Task.CompletedTask;
                    }
                };
            });


        }
    }
}
