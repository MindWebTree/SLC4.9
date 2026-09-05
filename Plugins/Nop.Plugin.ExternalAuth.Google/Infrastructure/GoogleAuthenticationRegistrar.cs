using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication.External;

namespace Nop.Plugin.ExternalAuth.Google.Infrastructure
{
    /// <summary>
    /// Represents registrar of Facebook authentication service
    /// </summary>
    public class GoogleAuthenticationRegistrar : IExternalAuthenticationRegistrar
    {
        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="builder">Authentication builder</param>
        public void Configure(AuthenticationBuilder builder)
        {
            builder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                //set credentials
                var settings = EngineContext.Current.Resolve<GoogleExternalAuthSettings>();
                options.ClientId = settings.ClientKeyIdentifier;
                options.ClientSecret = settings.ClientSecret;

                //store access and refresh tokens for the further usage
                options.SaveTokens = true;

                //set custom events handlers
                options.Events = new OAuthEvents
                {
                    //in case of error, redirect the user to the specified URL
                    OnRemoteFailure = async context =>
                    {
                            
                        context.HandleResponse();
                        string errorUrl = "/";
                        try
                        {
                            errorUrl = context.Properties.GetString(GoogleAuthenticationDefaults.ErrorCallback);

                        }
                        catch (Exception)
                        {

                            
                        }



                        var html = $@"
<html>
  <head><script>
    if (window.opener) {{
        window.opener.location.href = '{errorUrl}';
        window.close();
    }} else {{
        window.location.href = '{errorUrl}';
    }}
  </script></head>
  <body></body>
</html>";

                        context.Response.ContentType = "text/html";
                        await context.Response.WriteAsync(html);

                    }
                };
            });
        }
    }
}