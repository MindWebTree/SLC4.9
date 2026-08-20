using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Infrastructure
{
    public class CommunicatorHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public CommunicatorHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/AuthorizeNetHosted/communicator",
                StringComparison.OrdinalIgnoreCase))
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.Remove("X-Frame-Options");
                    context.Response.Headers.Remove("Content-Security-Policy");
                    context.Response.Headers.Remove("Content-Security-Policy-Report-Only");

                    context.Response.Headers["Content-Security-Policy"] =
       "default-src 'self' 'unsafe-inline' 'unsafe-eval' https://*.authorize.net; " +
       "frame-src 'self' https://test.authorize.net https://accept.authorize.net; " +
       "frame-ancestors 'self' https://test.authorize.net https://accept.authorize.net https://*.authorize.net;";

                    return Task.CompletedTask;
                });
            }

            await _next(context);
        }
    }
}
