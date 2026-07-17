using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models;
using MWT.Nop.Plugin.MegaMenu.Domain;
using MWT.Nop.Plugin.MegaMenu.Domain.Enums;
using MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Components
{
    public class MegaMenuItemTemplateAdminViewComponent : ViewComponent
    {

         
        private readonly ICategoryService _categoryService;
        private readonly ITopicService _topicService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;
        private readonly IProductTagService _productTagService;

        public MegaMenuItemTemplateAdminViewComponent(
          ICategoryService categoryService, 
          ITopicService topicService,
          IManufacturerService manufacturerService,
          IVendorService vendorService,
          IProductTagService productTagService)
        {
            this._categoryService = categoryService; 
            this._topicService = topicService;
            this._manufacturerService = manufacturerService;
            this._vendorService = vendorService;
            this._productTagService = productTagService;
        }

        public async Task<IViewComponentResult> InvokeAsync(MenuItemModel model)

        {

            if (MWT.Nop.Plugin.MegaMenu.Infrastructure.Constants.Plugin.PredefinedPageTypes.Contains(model.Type))
                return (IViewComponentResult)this.View<MenuItemModel>("~/Plugins/MWT.Nop.Plugin.MegaMenu/Areas/Admin/Views/MegaMenuItemAdmin/Templates/PredefinedPages.cshtml", model);
            model.IncludeInTopMenu = true;
            if (model.Type == MenuItemType.Categories)
            {
                if (model.EntityId == 0)
                {
                    model.IncludeInTopMenu = model.MaximumNumberOfEntities > 0;
                }
                else
                {
                    Category categoryByIdAsync = await this._categoryService.GetCategoryByIdAsync(model.EntityId);
                    if (categoryByIdAsync != null)
                    {
                        model.Title = categoryByIdAsync.Name;
                        model.IncludeInTopMenu = true;
                    }
                }
            }
            else if (model.Type == MenuItemType.Manufacturers)
            {
                if (model.EntityId == 0)
                {
                    model.IncludeInTopMenu = model.CatalogTemplate == CatalogTemplate.Simple || model.MaximumNumberOfEntities > 0;
                }
                else
                {
                    Manufacturer manufacturerByIdAsync = await this._manufacturerService.GetManufacturerByIdAsync(model.EntityId);
                    if (manufacturerByIdAsync != null)
                        model.Title = manufacturerByIdAsync.Name;
                }
            }
            else if (model.Type == MenuItemType.Vendors)
            {
                if (model.EntityId == 0)
                {
                    model.IncludeInTopMenu = model.CatalogTemplate == CatalogTemplate.Simple || model.MaximumNumberOfEntities > 0;
                }
                else
                {
                    Vendor vendorByIdAsync = await this._vendorService.GetVendorByIdAsync(model.EntityId);
                    if (vendorByIdAsync != null)
                        model.Title = vendorByIdAsync.Name;
                }
            }
            else if (model.Type == MenuItemType.Topics)
            {
                if (model.EntityId == 0)
                {
                    model.IncludeInTopMenu = model.MaximumNumberOfEntities > 0;
                }
                else
                {
                    Topic topicByIdAsync = await this._topicService.GetTopicByIdAsync(model.EntityId);
                    if (topicByIdAsync != null)
                    {
                        model.Title = string.Format("{0} ({1})", (object)topicByIdAsync.Title, (object)topicByIdAsync.SystemName);
                        model.IncludeInTopMenu = true;
                    }
                }
            }
            else if (model.Type == MenuItemType.ProductTags)
            {
                if (model.EntityId == 0)
                {
                    model.IncludeInTopMenu = model.CatalogTemplate == CatalogTemplate.Simple || model.MaximumNumberOfEntities > 0;
                }
                else
                {
                    ProductTag productTagByIdAsync = await this._productTagService.GetProductTagByIdAsync(model.EntityId);
                    if (productTagByIdAsync != null)
                        model.Title = productTagByIdAsync.Name;
                }
            }
            return (IViewComponentResult)this.View<MenuItemModel>("~/Plugins/MWT.Nop.Plugin.MegaMenu/Areas/Admin/Views/MegaMenuItemAdmin/Templates/" + model.Type.ToString() + ".cshtml", model);
        }
    }
}
