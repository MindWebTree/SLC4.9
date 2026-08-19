using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories.Catalog;
using MWT.Plugin.Misc.MwtStorefront.Infrastructure.Filters;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers.Api
{
    public class CatalogApiController : BasePublicController
    {
        #region fields

        private readonly ICustomCatalogModelFactory _catalogModelFactory;

        #endregion

        #region Ctor
        public CatalogApiController(ICustomCatalogModelFactory catalogModelFactory)
        {
            this._catalogModelFactory = catalogModelFactory;
        }

        #endregion

        #region Methods

        [ValidateApiKey]
        [HttpGet]
        public async Task<IActionResult> GetManufacturers()
        {
            return new JsonResult(await _catalogModelFactory.PrepareManufacturerAllModelsAsync());

        }

        #endregion
    }
}
