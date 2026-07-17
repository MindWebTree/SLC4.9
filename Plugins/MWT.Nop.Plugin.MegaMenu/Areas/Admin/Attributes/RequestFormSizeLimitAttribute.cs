using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequestFormSizeLimitAttribute : Attribute, IAuthorizationFilter, IFilterMetadata, IOrderedFilter
    {
        private readonly FormOptions _formOptions;
        public RequestFormSizeLimitAttribute(int valueCountLimit) => this._formOptions = new FormOptions()
        {
            ValueCountLimit = valueCountLimit
        };

        public int Order { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            IFeatureCollection features = context.HttpContext.Features;
            IFormFeature formFeature = features.Get<IFormFeature>();
            if (formFeature != null && formFeature.Form != null)
                return;
            features.Set<IFormFeature>((IFormFeature)new FormFeature(context.HttpContext.Request, this._formOptions));
        }
    }
}
