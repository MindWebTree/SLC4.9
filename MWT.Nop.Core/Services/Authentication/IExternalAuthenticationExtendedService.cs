using Microsoft.AspNetCore.Mvc;
using Nop.Services.Authentication.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Authentication
{
    public partial interface IExternalAuthenticationExtendedService: IExternalAuthenticationService
    {
        Task<IActionResult> CustomAuthenticateAsync(ExternalAuthenticationParameters parameters, string returnUrl = null);
    }
}
