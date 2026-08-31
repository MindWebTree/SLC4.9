using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Security;
using Nop.Core.Http.Extensions;
using Nop.Data;
using Nop.Services.Logging;
using Nop.Web.Framework.Security.Captcha;
using OfficeOpenXml.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Infrastructure.Filters
{
    public sealed class ValidateCaptchaExtendedAttribute : TypeFilterAttribute
    {
        #region Ctor

        /// <summary>
        /// Create instance of the filter attribute 
        /// </summary>
        /// <param name="actionParameterName">The name of the action parameter to which the result will be passed</param>
        public ValidateCaptchaExtendedAttribute(string actionParameterName = "captchaValid") : base(typeof(ValidateCaptchaExtendedFilter))
        {
            Arguments = [actionParameterName];
        }

        #endregion

        #region Nested filter

        /// <summary>
        /// Represents a filter enabling CAPTCHA validation
        /// </summary>
        private class ValidateCaptchaExtendedFilter : IAsyncActionFilter
        {
            #region Constants

            private const string RESPONSE_FIELD_KEY = "recaptcha_response_field";
            private const string G_RESPONSE_FIELD_KEY = "g-recaptcha-response";

            #endregion

            #region Fields

            protected readonly string _actionParameterName;
            protected readonly CaptchaHttpClient _captchaHttpClient;
            protected readonly CaptchaSettings _captchaSettings;
            protected readonly ILogger _logger;
            protected readonly IWorkContext _workContext;

            #endregion

            #region Ctor

            public ValidateCaptchaExtendedFilter(string actionParameterName,
                CaptchaHttpClient captchaHttpClient,
                CaptchaSettings captchaSettings,
                ILogger logger,
                IWorkContext workContext)
            {
                _actionParameterName = actionParameterName;
                _captchaHttpClient = captchaHttpClient;
                _captchaSettings = captchaSettings;
                _logger = logger;
                _workContext = workContext;
            }

            #endregion

            #region Utilities

            /// <summary>
            /// Called asynchronously before the action, after model binding is complete.
            /// </summary>
            /// <param name="context">A context for action filters</param>
            /// <returns>A task that represents the asynchronous operation</returns>
            private async Task ValidateCaptchaAsync(ActionExecutingContext context)
            {
                ArgumentNullException.ThrowIfNull(context);

                if (!DataSettingsManager.IsDatabaseInstalled())
                    return;

                //whether CAPTCHA is enabled
                if (_captchaSettings.Enabled)
                {
                    //push the validation result as an action parameter
                    var isValid = false;

                    //get form values
                    var captchaResponseValue = await context.HttpContext.Request.GetFormValueAsync(RESPONSE_FIELD_KEY);
                    var gCaptchaResponseValue = await context.HttpContext.Request.GetFormValueAsync(G_RESPONSE_FIELD_KEY);

                    if (!StringValues.IsNullOrEmpty(captchaResponseValue) || !StringValues.IsNullOrEmpty(gCaptchaResponseValue))
                    {
                        //validate request
                        try
                        {
                            var value = !StringValues.IsNullOrEmpty(captchaResponseValue) ? captchaResponseValue : gCaptchaResponseValue;
                            var response = await _captchaHttpClient.ValidateCaptchaAsync(value);

                            switch (_captchaSettings.CaptchaType)
                            {
                                case CaptchaType.CheckBoxReCaptchaV2:
                                    isValid = response.IsValid;
                                    break;

                                case CaptchaType.ReCaptchaV3:
                                    isValid = response.IsValid &&
                                              response.Action == context.RouteData.Values["action"].ToString() &&
                                              response.Score > _captchaSettings.ReCaptchaV3ScoreThreshold;
                                    break;

                                default:
                                    break;
                            }
                            if (!isValid)
                            {
                                var customer = await _workContext.GetCurrentCustomerAsync();

                                await _logger.InsertLogAsync(LogLevel.Error,
                                    $"CAPTCHA validation failed. ",
                                    $"Type: {_captchaSettings.CaptchaType}, " +
                                    $"Score: {response.Score}, " +
                                    $"Action: {response.Action}, " +
                                    $"captchaResponseValue: {value}, " +
                                    $"IP: {context.HttpContext.Connection.RemoteIpAddress}");
                            }
                        }
                        catch (Exception exception)
                        {
                            await _logger.ErrorAsync("Error occurred on CAPTCHA validation", exception, await _workContext.GetCurrentCustomerAsync());
                        }
                    }

                    context.ActionArguments[_actionParameterName] = isValid;
                }
                else
                    context.ActionArguments[_actionParameterName] = false;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called asynchronously before the action, after model binding is complete.
            /// </summary>
            /// <param name="context">A context for action filters</param>
            /// <param name="next">A delegate invoked to execute the next action filter or the action itself</param>
            /// <returns>A task that represents the asynchronous operation</returns>
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                await ValidateCaptchaAsync(context);
                if (context.Result == null)
                    await next();
            }

            #endregion
        }

        #endregion
    }
 
}
