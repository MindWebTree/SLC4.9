using MWT.Nop.Core.Domain.ProductBundle;
using MWTNop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Data;

namespace MWT.Nop.Core.Services.BundleProduct
{
    public class BundleLoggerService : IBundleLoggerService
    {
        private readonly IRepository<BundleAuditLog> _bundleAuditLogRepository;
        private readonly IWorkContext _workContext; 

        public BundleLoggerService(
            IRepository<BundleAuditLog> bundleAuditLogRepository,
            IWorkContext workContext)
        {
            _bundleAuditLogRepository = bundleAuditLogRepository;
            _workContext = workContext; 
        }

  
     
       

        public async Task LogBundleUpdatedAsync(BundleConfiguration newBundle,bool isValid, List<BundleItem> items
            , decimal? oldPrice , decimal? newPrice)
        {
            var user = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
          
            int actualQuantity = items?.Sum(x => x.Quantity) ?? 0;
            string status = isValid ? "VALID" : "INVALID";
             
            List<string> changes = new List<string>();
            string actionType = "Updated";

            if (oldPrice != newPrice)
            {
                changes.Add($"Price Changed: {oldPrice} -> {newPrice}");
                actionType = "PriceUpdated";
            }

            //if (oldBundle.NoOfPieces != newBundle.NoOfPieces)
            //{
            //    changes.Add($"Required Pieces Changed: {oldBundle.NoOfPieces} -> {newBundle.NoOfPieces}");
            //}

            // Check if validity changed (e.g. an item was added or removed)
           // bool wasValid = BundleService.IsBundleValid(oldBundle); // Note: This uses the CURRENT items, which isn't perfect for tracking 'past' validity, but works for the current state check.
            //if (!wasValid && isValid)
            //{
            //    changes.Add("Status became VALID");
            //    actionType = "ValidityChanged";
            //}
            //else if (wasValid && !isValid)
            //{
            //    changes.Add("Status became INVALID");
            //    actionType = "ValidityChanged";
            //}

            string changeSummary = changes.Any() ? string.Join(" | ", changes) : "General Update";
            string message = $"Bundle Updated. Status: {status}. {changeSummary}. Required Pieces: {newBundle.NoOfPieces}, Actual Qty: {actualQuantity}.";

            var logEntry = new BundleAuditLog
            {
                BundleId = newBundle.Id,
                ProductId = newBundle.ProductId, // Ensure your BundleConfiguration domain has this!
                VariantId = newBundle.VariantId,
                ActionType = actionType,
                UserIdentifier = user,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                IsValid = isValid,
                RequiredPieces = newBundle.NoOfPieces,
                ActualQuantity = actualQuantity,
                LogMessage = message,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _bundleAuditLogRepository.InsertAsync(logEntry);
        }
        public async Task LogBundleDeletedAsync(BundleConfiguration bundle , VariantCombination variant)
        {
            var user = (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;
            string message = $"Bundle Deleted. Required Pieces: {bundle.NoOfPieces}. Final Price: {variant.Price}";

            var logEntry = new BundleAuditLog
            {
                BundleId = bundle.Id,
                ProductId = bundle.ProductId,
                VariantId = bundle.VariantId,
                ActionType = "Deleted",  
                UserIdentifier = user,
                OldPrice = variant.Price,
                NewPrice = null,
                IsValid = false, 
                RequiredPieces = bundle.NoOfPieces,
                ActualQuantity = 0,
                LogMessage = message,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _bundleAuditLogRepository.InsertAsync(logEntry);
        }
        public async Task LogBundleCreatedAsync(BundleConfiguration bundle)
        {
            var user =  (await _workContext.GetCurrentCustomerAsync())?.Id ?? 0;

            string message = $"Bundle Created. Status: INVALID. Required Pieces: {bundle.NoOfPieces}.";

            var logEntry = new BundleAuditLog
            {
                BundleId = bundle.Id,
                ProductId = bundle.ProductId,
                VariantId = bundle.VariantId,
                ActionType = "Created",
                UserIdentifier = user,
                OldPrice = null,
                NewPrice = null,
                IsValid = false,
                RequiredPieces = bundle.NoOfPieces,
                ActualQuantity = 0,
                LogMessage = message,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _bundleAuditLogRepository.InsertAsync(logEntry);
        }

        public async Task<IPagedList<BundleAuditLog>> GetAllLogsAsync(
            int productId = 0,
            int variantId = 0,
            string searchKeyword = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _bundleAuditLogRepository.Table;

            // 1. Check if the user typed a number (Product ID) into the search box
            bool isKeywordNumeric = int.TryParse(searchKeyword, out int parsedId);

            if (isKeywordNumeric && parsedId > 0)
            {
                // If a Product ID was searched, filter ONLY by this ID.
                // This ignores the 'productId' and 'variantId' parameters entirely.
                query = query.Where(log => log.ProductId == parsedId || log.VariantId == parsedId);
            }
            else
            {
                // 2. If it's not a numeric search, apply the standard parameters
                if (productId > 0)
                {
                    query = query.Where(log => log.ProductId == productId);
                }

                if (variantId > 0)
                {
                    query = query.Where(log => log.VariantId == variantId);
                }
                 
                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    query = query.Where(log =>
                        log.LogMessage != null && log.LogMessage.Contains(searchKeyword) || 
                        log.ActionType != null && log.ActionType.Contains(searchKeyword)
                    );
                }
            }

            query = query.OrderByDescending(log => log.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
    }
}
