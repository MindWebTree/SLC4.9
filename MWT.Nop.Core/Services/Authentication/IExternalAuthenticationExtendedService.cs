using Microsoft.AspNetCore.Mvc;
using Nop.Services.Authentication.External;

namespace MWT.Nop.Core.Services.Authentication
{
    public partial interface IExternalAuthenticationExtendedService: IExternalAuthenticationService
    {
        Task<IActionResult> CustomAuthenticateAsync(ExternalAuthenticationParameters parameters, string returnUrl = null);
    }
}
