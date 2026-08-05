using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Web.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.Catalog
{
    public interface ICustomCatalogModelFactory : ICatalogModelFactory
    {
        Task<List<CustomCategoryModel>> CustomPrepareHomepageCategoryModelsAsync();

    }
}
