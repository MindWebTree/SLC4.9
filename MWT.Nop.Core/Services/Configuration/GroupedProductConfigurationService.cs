using MWT.Nop.Core.Domain;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Configuration
{
    public partial class GroupedProductConfigurationService : IGroupedProductConfigurationService
    {

        private readonly IRepository<GroupedProductConfiguration> _groupedProductConfigurationRepository;
        #region Ctor

        public GroupedProductConfigurationService(IRepository<GroupedProductConfiguration> groupedProductConfigurationRepository)
        {
            _groupedProductConfigurationRepository = groupedProductConfigurationRepository;
        }

        #endregion

        #region Methods
        public async Task<List<GroupedProductConfiguration>> GetConfigurationOfGroupedProduct(int productId)
        {
            return await this._groupedProductConfigurationRepository.Table.Where(g => g.ProductId == productId).ToListAsync();
        }

        public async Task UpdateGroupProductConfiguration(GroupedProductConfiguration group)
        {
            if (group.Id != 0)
            {
                await this._groupedProductConfigurationRepository.UpdateAsync(group);
            }
            else
            {
                var configurations = await this._groupedProductConfigurationRepository.Table.Where(c => c.ProductId == group.ProductId && c.ProductAttributeOptionId == group.ProductAttributeOptionId).ToListAsync();

                foreach (var configuration in configurations)
                {
                    await this._groupedProductConfigurationRepository.DeleteAsync(configuration);
                }
                await this._groupedProductConfigurationRepository.InsertAsync(group);
            }
        }

        public async Task DeleteGroupProductConfiguration(GroupedProductConfiguration group)
        {
            await _groupedProductConfigurationRepository.DeleteAsync(group);
        }
        #endregion
    }
}
