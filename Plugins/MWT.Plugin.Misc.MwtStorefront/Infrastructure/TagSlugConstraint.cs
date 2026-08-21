using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MWT.Nop.Core.Services.TagPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Infrastructure
{
    public class TagSlugConstraint : IRouteConstraint
    {
        private static readonly HashSet<string> _reserved = new(StringComparer.OrdinalIgnoreCase)
    {
        "products"
    };
        public bool Match(
            Microsoft.AspNetCore.Http.HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (!values.TryGetValue("tagSlug", out var slugVal))
                return false;

            var slug = slugVal?.ToString();
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            if (_reserved.Contains(slug))
                return false;
            var svc = httpContext.RequestServices.GetService<ITagSlugService>();
            if (svc == null) return false;

            // Sync over async — safe here because it's a cache hit
            return svc.IsValidSlugAsync(slug).GetAwaiter().GetResult();
        }
    }
}
