
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Service.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.ScheduleTasks;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public partial class ConfigurationProductPriceImportTask : IScheduleTask
    {

        #region Fields

        private readonly IProductExtendedService _productService;
        private readonly IRepository<ConfigurationProductPriceImport> _configurationProductPriceImportRepository;

        #endregion

        #region Ctor

        public ConfigurationProductPriceImportTask(IProductExtendedService productService,
            IRepository<ConfigurationProductPriceImport> configurationProductPriceImportRepository)
        {
            _productService = productService;
            _configurationProductPriceImportRepository = configurationProductPriceImportRepository;
        }

        #endregion
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var records = await (from record in _configurationProductPriceImportRepository.Table
                                 where !record.IsUpdated
                                 orderby record.ProductId
                                 select record).ToListAsync();
            foreach (var record in records)
            {
                try
                {
                    var variants = await _productService.GetProductVariants(record.ProductId);
                    var variant = variants.Where(v => v.VariantId == record.VariantId).FirstOrDefault();
                    if (variant != null)
                    {
                        variant.OldPrice = record.OlPrice;
                        variant.Price = record.Price;
                        variant.Msrp = variant.Msrp;
                        variant.Title = variant.Title;
                        variant.EnableSurcharge = variant.EnableSurcharge;
                        variant.EstimatedDeliveryDate = variant.EstimatedDeliveryDate;
                        variant.WgsRequired = variant.WgsRequired;
                        await _productService.UpdateVariant(variant);
                        record.IsUpdated = true;
                        await _configurationProductPriceImportRepository.UpdateAsync(record);
                    }
                }
                catch (Exception exp)
                {

                }
            }

        }
    }
}
