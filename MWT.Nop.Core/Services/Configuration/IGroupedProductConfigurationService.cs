using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

