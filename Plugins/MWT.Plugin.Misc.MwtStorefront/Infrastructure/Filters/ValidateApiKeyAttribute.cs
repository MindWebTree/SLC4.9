using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Services.Configuration;

namespace MWT.Plugin.Misc.MwtStorefront.Infrastructure.Filters
{
    public sealed class ValidateApiKeyAttribute : TypeFilterAttribute
    {
        #region Ctor

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public ValidateApiKeyAttribute() : base(typeof(ValidateApiKeyFilter))
        {
        }

        #endregion

        #region Nested filter

        /// <summary>
        /// Represents a filter that checks WWW at the beginning of the URL and properly redirect if necessary
        /// </summary>
        private class ValidateApiKeyFilter : IAsyncAuthorizationFilter
        {
            #region Fields

            private readonly ISettingService _settingService;


            #endregion

            #region Ctor

            public ValidateApiKeyFilter(ISettingService settingService)
            {
                _settingService = settingService;
            }

            #endregion

            #region Utilities




            #endregion

            #region Methods

            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                if (context.HttpContext.Request.Headers["ApiKey"].Count == 0)
                {
                    throw new Exception("Necessary AiKey HTTP headers not present!");
                }
                else
                {

                    if (context.HttpContext.Request.Headers["ApiKey"] != await _settingService.GetSettingByKeyAsync<string>("Api.Key"))
                        throw new Exception("AiKey is not valid!");


                }
            }

            #endregion
        }

        #endregion
    }
}
