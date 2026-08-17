using MWT.Nop.Core.Domain.Custom.Integrity_Report;
using Nop.Core.Domain.Catalog;
using Nop.Data;

namespace MWT.Nop.Core.Services.Integrity_Report
{
    public partial class ProductIntegrityReportService : IProductIntegrityReportService
    {
        #region Fields

        private IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        private IRepository<ProductAttributeValue> _productAttributeValueRepository;
        private IRepository<ProductAttribute> _productAttribute;
        private IRepository<Product> _productRepository;
        private IRepository<ProductIntegrityReport> _productIntegrityReportRepository;


        #endregion

        #region Ctor

        public ProductIntegrityReportService(IRepository<ProductAttributeMapping> productAttributeMappingRepository,
            IRepository<ProductAttributeValue> productAttributeValueRepository,
            IRepository<Product> productRepository, IRepository<ProductAttribute> productAttribute,
            IRepository<ProductIntegrityReport> productIntegrityReportRepository)
        {
            this._productAttributeMappingRepository = productAttributeMappingRepository;
            this._productAttributeValueRepository = productAttributeValueRepository;
            this._productRepository = productRepository;
            this._productAttribute = productAttribute;
            this._productIntegrityReportRepository = productIntegrityReportRepository;
        }

        #endregion

        public virtual async Task<IList<(Product Product, string issue)>> GetUnpublishedOrAttributeMissingProductsAsync(List<Product> prouctList)
        {
            var query =
                from p in prouctList
                where p.Published

                from pam in _productAttributeMappingRepository.Table
                where pam.ProductId == p.Id && pam.IsRequired

                join pa in _productAttribute.Table
                    on pam.ProductAttributeId equals pa.Id

                join pav in _productAttributeValueRepository.Table
                    on pam.Id equals pav.ProductAttributeMappingId into pavGroup

                where !pavGroup.Any() || pavGroup.All(v => !v.Published) || pavGroup.All(v => !v.IsPreSelected)

                select new
                {
                    Product = p,
                    AttributeName = pa.Name
                };

            var result = await query.ToListAsync();

            return result.GroupBy(x => x.Product.Id)
                .Select(g => (Product: g.First().Product, issue: "Issue in Attribute Setup " + string.Join(", ", g
                .Select(x => x.AttributeName)
                .Distinct()))).ToList();
        }

        public async Task DeleteAllExceptIdsAsync(List<int> ids)
        {
            // Get all records that should be deleted
            var query = _productIntegrityReportRepository.Table.AsQueryable();

            if (ids != null && ids.Any())
                query = query.Where(x => !ids.Contains(x.ProductId));

            var recordsToDelete = await query.ToListAsync();

            if (!recordsToDelete.Any())
                return;

            const int batchSize = 50;

            for (int i = 0; i < recordsToDelete.Count; i += batchSize)
            {
                var batch = recordsToDelete.Skip(i).Take(batchSize).ToList();
                await _productIntegrityReportRepository.DeleteAsync(batch);
            }
        }


        public async Task Upsert(List<(Product Product, string ErrorReason)> combinedlist)
        {

            try
            {
                ProductIntegrityReport productIntegrityReport = new ProductIntegrityReport();
                foreach (var item in combinedlist)
                {
                    productIntegrityReport = await _productIntegrityReportRepository.Table
                        .FirstOrDefaultAsync(x => x.ProductId == item.Product.Id);

                    if (productIntegrityReport != null)
                    {
                        // UPDATE
                        productIntegrityReport.ErrorReason = item.ErrorReason;
                        await _productIntegrityReportRepository.UpdateAsync(productIntegrityReport);
                    }
                    else
                    {
                        // INSERT
                        productIntegrityReport = new ProductIntegrityReport();
                        productIntegrityReport.ProductName = item.Product.Name;
                        productIntegrityReport.ErrorReason = item.ErrorReason;
                        productIntegrityReport.ProductId = item.Product.Id;
                        await _productIntegrityReportRepository.InsertAsync(productIntegrityReport);
                    }
                }
            }
            catch (System.Exception ex)
            {

                throw;
            }
        }
        public async Task<List<ProductIntegrityReport>> GetAllAsync()
        {
            return await _productIntegrityReportRepository.Table
                .OrderBy(x => x.Id)
                .ToListAsync();
        }
    }
}
