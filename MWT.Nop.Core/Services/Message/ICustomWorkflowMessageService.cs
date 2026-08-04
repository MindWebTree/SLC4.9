

using MWT.Nop.Core.Domain.PhoneOrder;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Services.Messages;
namespace MWT.Nop.Core.Services.Message
{
    public partial interface ICustomWorkflowMessageService: IWorkflowMessageService
    {
        Task<IList<int>> SendCustomizationFormMessageAsync(int languageId, string senderEmail,
     string senderName, string subject, string body);

        Task<IList<int>> CustomSendOrderPlacedStoreOwnerNotificationAsync(Order order, int languageId);

        Task<IList<int>> CustomSendOrderPlacedCustomerNotificationAsync(Order order, int languageId, string attachmentFilePath = null, string attachmentFileName = null, bool customerOnly = false);

        Task<IList<int>> CustomSendOrderPlacedVendorNotificationAsync(Order order, Vendor vendor, int languageId);

        Task<IList<int>> CustomSendOrderPlacedAffiliateNotificationAsync(Order order, int languageId);
        Task<IList<int>> CustomOrder_SendPaymentLinkNotificationAsync(CustomOrder order, int languageId,string email="");

        Task<IList<int>> CustomOrder_SendCustomerNotificationAsync(CustomOrder order, int languageId, bool customerOnly = false);
        Task<IList<int>> CustomOrder_SendPartialPaymentLinkNotificationAsync(CustomOrder order, int languageId, string email = "");

        Task<IList<int>> SendCustomFormMessageAsync(int languageId, string customerEmailAddress, string customerName, string bccaddress, string subject, string body);

        //Task<IList<int>> CustomOrder_SendPartialOrderLinkNotificationAsync(CustomOrder order, int languageId);
        #region Estimated Shipping Date Module
        Task<IList<int>> SendSupportNotificationZipCodeNotFound(int languageId, Store store
            , string zipCode, string ipAddress, string customerName, string customerEmail);

        #endregion

        #region Back in Stock Notification
        Task<IList<int>> CustomSendBackInStockNotificationAsync(BackInStockSubscription subscription, int languageId, List<Token> tokens);
        #endregion

        #region Customer
        Task<IList<int>> OldCustomerSendCustomerPasswordRecoveryMessageAsync(Customer customer, int languageId);
        #endregion

        #region Receipts
        Task<string> OrderReceiptContentAsync(Order order, int languageId);
        Task<string> CustomOrderInvoiceContentNotificationAsync(CustomOrder order, int languageId);
        Task<string> CustomOrderReceiptContentAsync(CustomOrder order, int languageId);



        #endregion

        #region Order Decline

        Task<List<int>> SendOrderDeclineMessage(Customer customer,int languageId,string errorMessage,int orderId);

        #endregion

        #region Wishlist
        Task<IList<int>> CustomSendWishlistEmailAFriendMessageAsync(Customer customer, int languageId,
          string customerEmail, string friendsEmail, string personalMessage);
        #endregion

        #region Abandoned Card
        Task<List<int>> SendSupportAbandonedCartEmailMessage(Customer customer, int[] cartItems, int languageId);

        Task<bool> SendAbandonedCartReminderNotificationAsync(Customer customer, string name, string email, int messageTemplateId, string cartLink, Product product, List<Product> 
            relatedProducts, int languageId,string utmSource);
        #endregion

        #region Purchase Journey
        Task<(bool, string)> SendPurchaseJourneyNotificationAsync(Customer customer, List<Product> products, int productId, int categoryId, string templatetype, int messageTemplateId,string utm_params, int languageId);

        #endregion
    }
}
