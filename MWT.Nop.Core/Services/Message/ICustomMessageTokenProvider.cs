
using MWT.Nop.Core.Domain.CustomOrders;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Messages;

namespace MWT.Nop.Core.Services.Message 
{
    /// <summary>
    /// Message token provider
    /// </summary>
    public partial interface ICustomMessageTokenProvider : IMessageTokenProvider
    {
        Task CustomAddOrderTokensAsync(IList<Token> tokens, Order order, int languageId, int vendorId = 0);
        Task AddStoreLogoToken(IList<Token> tokens);
        Task CustomOrderAddTokensAsync(IList<Token> tokens, CustomOrder order, int languageId, int vendorId = 0);

        Task CustomAddCustomerTokensAsync(IList<Token> tokens, int customerId);
        void CustomAddShippingToken(IList<Token> tokens, int languageId, string zipCode, string ipAddress, string customerName,
             string customerEmail);

        Task CustomAddOrderDeclineTokensAsync(IList<Token> tokens,Customer customer,IList<ShoppingCartItem> cart,string error,int orderId,int languageId);

        Task CustomAddCustomerOrderDeclineTokensAsync(IList<Token> tokens, Customer customer, IList<ShoppingCartItem> cart, int orderId, int languageId);
        Task CustomSupportAddAbandonedCartTokensAsync(IList<Token> tokens, Customer customer, IList<ShoppingCartItem> cart, int languageId);

       Task CustomAddAbandonedCartTokensAsync(IList<Token> tokens, Customer customer,string cartLink, Product product, List<Product> relatedProducts,int languageId,string utmSource);

        Task WgsAdditionalServiceAddTokenAsync(IList<Token> tokens, CustomOrder customOrder, int languageId, int vendorId = 0);
        Task CustomAddPurchaseJourneyTokenAsync(IList<Token> tokens, Customer customer, List<Product> products, int productId, int categoryId, string templateType,string utm_params);

        Task CustomSupportAddPaymentIssueTokensAsync(IList<Token> tokens, string transactionId, decimal paidAmount, decimal expectedAmount,
        string PaymentMethod);

        Task CustomAddPendingOrderTokens(IList<Token> tokens, string transactionId, string orderId,
    bool isCustomOrder, int customOrderNumber, string paymentMethod);
    }
}
