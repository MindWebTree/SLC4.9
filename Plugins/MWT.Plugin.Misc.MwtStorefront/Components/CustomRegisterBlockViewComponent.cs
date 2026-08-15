using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class CustomRegisterBlockViewComponent : NopViewComponent
    {

        #region Fields

        private readonly ICustomerModelFactory _customerModelFactory;

        #endregion

        #region Ctor

        public CustomRegisterBlockViewComponent(ICustomerModelFactory customerModelFactory)
        {
            _customerModelFactory = customerModelFactory;
        }

        #endregion

        #region Methods
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await _customerModelFactory.PrepareRegisterModelAsync(new RegisterModel(), false, setDefaultValues: true));
        }

        #endregion
    }
}
