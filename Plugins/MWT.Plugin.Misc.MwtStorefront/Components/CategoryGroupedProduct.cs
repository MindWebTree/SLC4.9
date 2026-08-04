using Microsoft.AspNetCore.Mvc;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Components
{
    public class CategoryGroupedProductsViewComponent : NopViewComponent
    {
        #region Fields

        private readonly ICustomProductModelFactory  _productModelFactory;

        #endregion

        #region Ctor

        public CategoryGroupedProductsViewComponent(ICustomProductModelFactory productModelFactory)
        {
            _productModelFactory = productModelFactory;
        }

        #endregion

        #region Methods
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(int entityId)
        {

            return View(await _productModelFactory.GetCategoryGroupedProducts(entityId));
        }

        #endregion
    }
}

