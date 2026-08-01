using MWTNop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.Catalog
{
    public interface IVariantModelFactory
    {
        Task<VariantCombination> ValidateVariantID(int productId, int variantId);
        Task<int> GetVariantIdBySize(int productId, string size);
        Task<bool> IsVariantSurchargeApplicable(int variantId);
    }
}
