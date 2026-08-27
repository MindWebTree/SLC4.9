using Microsoft.AspNetCore.Mvc.Filters;
using MWT.Nop.Core.Domain.Security;
using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace MWT.Nop.Core.Services.Security
{
    /// <summary>
    /// Permission service interface
    /// </summary>
    public partial interface IPermissionExtendedService : IPermissionService
    {
        Task<bool> CustomAuthorizeAsync(string permissionRecordSystemName, Customer customer, AuthorizationFilterContext context);

        #region Category Permission

        Task<IList<CategoryUserMapping>> GetAllCategoryUserMappingAsync();
        Task DeleteCategoryPermissionAsync(CategoryUserMapping categoryUserMapping);
        Task InsertCategoryPermissionAsync(CategoryUserMapping categoryUserMapping);
        Task<CategoryUserMapping> GetCategoryPermissionByIdAsync(int categoryUserMappingId);
        Task<bool> HasCategoryPermission(int categoryId, int userId);


        #endregion

    }
}
