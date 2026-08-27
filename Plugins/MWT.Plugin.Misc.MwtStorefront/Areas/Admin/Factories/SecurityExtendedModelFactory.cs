using Microsoft.AspNetCore.Mvc.Rendering;
using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.Security;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Security;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories
{
    public partial class SecurityModelExtendedFactory : ISecurityModelExtendedFactory
    {
        private readonly IPermissionExtendedService _permissionService;
        private readonly ICustomerExtendedService  _customerService;
        public SecurityModelExtendedFactory(IPermissionExtendedService permissionService , ICustomerExtendedService customerService)
        {
            _permissionService = permissionService;
            _customerService = customerService;
        }
        #region  Category Permission

        public async Task<CategoryPermissionListModel> PrepareCategoryPermissionListModelAsync(CategoryPermissionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Get mappings and convert to paginated list
            var categoryUserTerms = (await _permissionService.GetAllCategoryUserMappingAsync())
                .ToPagedList(searchModel);
            var _categoryService = EngineContext.Current.Resolve<ICategoryService>();
            // Prepare grid model
            var model = await new CategoryPermissionListModel().PrepareToGridAsync(
                searchModel,
                categoryUserTerms,
                () => categoryUserTerms.SelectAwait(async categoryUser =>
                {
                  
                    var customer = await _customerService.GetCustomerByIdAsync(categoryUser.UserId);
                    var category = await _categoryService.GetCategoryByIdAsync(categoryUser.CategoryId);
                    var categoryModel = categoryUser.ToModel<CategoryPermissionModel>();
                    categoryModel.CategoryName = category.Name;
                    categoryModel.UserEmail = customer.Email;
                    return categoryModel;
                })
            );

            return model;
        }

        public virtual async Task PrepareCustomersAsync(IList<SelectListItem> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            // Get available customers
            var availableCustomerItems = await _customerService.GetAllCategoryManagers();

            // Add to the list
            foreach (var customerItem in availableCustomerItems)
            {
                items.Add(new SelectListItem
                {
                    Value = customerItem.Id.ToString(),
                    Text = customerItem.Email
                });
            }
        }
        #endregion
    }
}