using MWT.Nop.Core.Domain;

namespace MWT.Nop.Core.Services.Configuration
{
    public  partial interface IGroupedProductConfigurationService
    {
        Task<List<GroupedProductConfiguration>> GetConfigurationOfGroupedProduct(int productId);

        Task UpdateGroupProductConfiguration(GroupedProductConfiguration group);
        Task DeleteGroupProductConfiguration(GroupedProductConfiguration group);
    }
}

