using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using MWT.Nop.Core.Domain.Security;
using MWT.Nop.Core.Services.Customers;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
namespace MWT.Nop.Core.Services.Security
{
    /// <summary>
    /// Permission service
    /// </summary>
    public partial class PermissionExtendedService : PermissionService, IPermissionExtendedService
    {
 

        public PermissionExtendedService(ICustomerService customerService, ILocalizationService localizationService, IRepository<CustomerRole> customerRoleRepository, IRepository<PermissionRecord> permissionRecordRepository, IRepository<PermissionRecordCustomerRoleMapping> permissionRecordCustomerRoleMappingRepository, IStaticCacheManager staticCacheManager, ITypeFinder typeFinder, IWorkContext workContext) : base(customerService, localizationService, customerRoleRepository, permissionRecordRepository, permissionRecordCustomerRoleMappingRepository, staticCacheManager, typeFinder, workContext)
        {
  
        }


        #region Methods

        public async Task<bool> CustomAuthorizeAsync(string permissionRecordSystemName, Customer customer, AuthorizationFilterContext context)
        {


            if (string.IsNullOrEmpty(permissionRecordSystemName))
                return false;
            bool result = false;
            var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
            var _categoryUserMappingRepository = EngineContext.Current.Resolve<IRepository<CategoryUserMapping>>();
            var _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();
            var _categoryService = EngineContext.Current.Resolve<ICategoryService>();
            var _productService = EngineContext.Current.Resolve<IProductService>();

            foreach (var role in customerRoles)
                if (await AuthorizeAsync(permissionRecordSystemName, role.Id))
                {
                    //yes, we have such permission
                    result = true;
                    break;
                }


            if (result == true)
            {

                if (permissionRecordSystemName == "ManageCategories" &&
                        await _customerService.IsInCustomerRoleAsync(customer, "CategoryManager"))
                {

                    int categoryId = await ExtractEntityIdAsync(context, "CategoryId");
                    if (categoryId > 0 && (await _categoryService.GetCategoryByIdAsync(categoryId)) != null)
                    {
                        var allowedCategoryIds = _categoryUserMappingRepository.Table
                                                 .Where(x => x.UserId == customer.Id)
                                                  .Select(x => x.CategoryId)
                                                   .ToList();
                        if (allowedCategoryIds.Where(a => a == categoryId).Any() == false)
                        {

                        }

                        return allowedCategoryIds.Where(a => a == categoryId).Any();
                    }


                }
                else if (permissionRecordSystemName == "ManageProducts" &&
                        await _customerService.IsInCustomerRoleAsync(customer, "CategoryManager"))
                {

                    int productId = await ExtractEntityIdAsync(context, "ProductId");
                    if (productId > 0 && (await _productService.GetProductByIdAsync(productId)) != null)
                    {
                        var allowedCategoryIds = _categoryUserMappingRepository.Table
                                                 .Where(x => x.UserId == customer.Id)
                                                  .Select(x => x.CategoryId)
                                                   .ToList();


                        var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(productId);
                        var test = allowedCategoryIds.Intersect(productCategories.Select(c => c.Id));
                        return allowedCategoryIds.Intersect(productCategories.Select(c => c.CategoryId)).Any();
                    }
                }
            }

            return result;

        }


        #endregion

        #region Utilities

        private async Task<int> ExtractEntityIdAsync(AuthorizationFilterContext context, string propName)
        {
            var request = context.HttpContext.Request;

            // 1️⃣ Try query string or route
            string categoryIdStr =
                request.Query[propName].FirstOrDefault()
                ?? context.RouteData.Values[propName]?.ToString();
            if (string.IsNullOrEmpty(categoryIdStr))
            {
                if (context.RouteData.Values.TryGetValue(propName, out var routeValue))
                    categoryIdStr = routeValue?.ToString();
                else if (context.RouteData.Values.TryGetValue("id", out var routeId))
                    categoryIdStr = routeId?.ToString();
            }
            if (string.IsNullOrEmpty(categoryIdStr))
            {
                categoryIdStr =
                request.Query["EntityId"].FirstOrDefault()
                ?? context.RouteData.Values["EntityId"]?.ToString();
            }
            if (int.TryParse(categoryIdStr, out int idFromQuery))
                return idFromQuery;

            // 2️⃣ Try form data
            if (request.HasFormContentType && request.Form.ContainsKey(propName))
            {
                if (int.TryParse(request.Form[propName], out int idFromForm))
                    return idFromForm;
            }


            if (request.ContentType?.Contains("application/json") == true)
            {
                request.EnableBuffering();
                using (var reader = new StreamReader(request.Body, leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();
                    request.Body.Position = 0;

                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        try
                        {
                            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
                            if (json != null && json.TryGetValue(propName, out var value))
                            {
                                if (int.TryParse(value?.ToString(), out int idFromJson))
                                    return idFromJson;
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }


            return 0;
        }

        #endregion

        #region  Category Permission

        public virtual async Task<IList<CategoryUserMapping>> GetAllCategoryUserMappingAsync()
        {
            var _categoryUserMappingRepository = EngineContext.Current.Resolve<IRepository<CategoryUserMapping>>();
            var query = from rq in _categoryUserMappingRepository.Table
                        orderby rq.CategoryId
                        select rq;
            return await query.ToListAsync();
        }

        public virtual async Task DeleteCategoryPermissionAsync(CategoryUserMapping categoryUserMapping)
        {
            var _categoryUserMappingRepository = EngineContext.Current.Resolve<IRepository<CategoryUserMapping>>();
            await _categoryUserMappingRepository.DeleteAsync(categoryUserMapping);
        }

        public virtual async Task<CategoryUserMapping> GetCategoryPermissionByIdAsync(int categoryUserMappingId)
        {
            var _categoryUserMappingRepository = EngineContext.Current.Resolve<IRepository<CategoryUserMapping>>();
            return await _categoryUserMappingRepository.GetByIdAsync(categoryUserMappingId, cache => default);
        }



        public virtual async Task InsertCategoryPermissionAsync(CategoryUserMapping categoryUserMapping)
        {
            var _categoryUserMappingRepository = EngineContext.Current.Resolve<IRepository<CategoryUserMapping>>();
            await _categoryUserMappingRepository.InsertAsync(categoryUserMapping);
        }


        public async Task<bool> HasCategoryPermission(int categoryId, int userId)
        {
            var _categoryUserMappingRepository = EngineContext.Current.Resolve<IRepository<CategoryUserMapping>>();
            return await (from c in _categoryUserMappingRepository.Table
                          where c.CategoryId == categoryId
                          && c.UserId == userId
                          select c).AnyAsync();
        }


        #endregion
    }
}
