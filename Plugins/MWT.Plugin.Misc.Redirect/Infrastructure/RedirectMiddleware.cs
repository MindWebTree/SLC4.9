using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

using Nop.Core;

using Nop.Core.Caching;
using Nop.Plugin.Misc.Redirect.Models;
using Nop.Plugin.Misc.Redirect.Models.Redirections;
using Nop.Plugin.Misc.Redirect.Services;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Redirect.Infrastructure
{
    public static class CustomExtensions
    {
        public static void UseCustomRedirect(this IApplicationBuilder application)
        {
            application.UseMiddleware<RedirectMiddleware>();
        }
    }

    public class RedirectMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;

        #endregion

        #region Ctor

        public RedirectMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        #endregion

       
        #region Methods

        /// <summary>
        /// Invoke middleware actions
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InvokeAsync(HttpContext context, IRedirectionsService _redirectionService, ILogger _logger)

        {
            try
            {
                if (context.Request.Method == HttpMethods.Get)
                {
                    (string redirectUrl, bool isPermanentRedirect) = await _redirectionService.ResolveRedirection(context.Request);
                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        var parsed = HttpUtility.UrlEncode(redirectUrl);
                        parsed = WebUtility.UrlDecode(parsed);

                        context.Request.Path = parsed;
                        context.Response.Redirect(parsed, isPermanentRedirect);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Redirect", ex);
            }


            await _next(context);
        }

        #endregion
    }
}


