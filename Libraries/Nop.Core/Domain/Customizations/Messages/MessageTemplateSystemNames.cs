namespace Nop.Core.Domain.Messages
{
    /// <summary>
    /// Represents message template system names
    /// </summary>
    public static partial class MessageTemplateSystemNames
    {
        public const string CustomizationFormLeads = "Service.CustomizationFormLeads";

        public const string CustomOrderPlacedStoreOwnerNotification = "Custom.OrderPlaced.StoreOwnerNotification";

        public const string CustomOrderPlacedCustomerNotification = "Custom.OrderPlaced.CustomerNotification";
        public const string CustomOrderPlacedReceipt = "Custom.OrderPlaced.Receipt";

        public const string CustomOrderPlacedVendorNotification = "Custom.OrderPlaced.VendorNotification";

        public const string CustomOrderPlacedAffiliateNotification = "Custom.OrderPlaced.AffiliateNotification";

        #region CustomOrder
        public const string CustomOrder_AdditionalService_InvoiceNotification = "CustomOrder.AdditionalService.InvoiceNotification";
        public const string CustomOrder_InvoiceNotification = "CustomOrder.InvoiceNotification";
        public const string CustomOrder_InvoiceReceipt = "CustomOrder.Invoice.Receipt";
        public const string CustomOrder_CustomerNotification = "CustomOrder.CustomerNotification";
        public const string CustomOrder_WGS_CustomerNotification = "CustomOrder.WGS.CustomerNotification";
        public const string CustomOrder_WGS_Complementory_CustomerNotification = "CustomOrder_WGS_Complementory_CustomerNotification";
        public const string CustomOrder_Receipt = "CustomOrder.Receipt";
        public const string CustomOrder_WGS_Receipt = "CustomOrder.WGS.Receipt";
        public const string CustomOrder_PartialOrderInvoiceNotification = "CustomOrder.PartialOrderInvoiceNotification";

        public const string CustomOrder_PartialOrderLinkNotification = "CustomOrder.PartialOrderLinkNotification";
        #endregion

        #region Estimated Delivery Date Module

        public const string Support_Delivery_Estimation_NotFound_Notification = "Support.Delivery.Estimation.NotFound.Notification";

        #endregion

        #region Customer
        public const string OldCustomerPasswordRecoveryMessage = "OldCustomer.PasswordRecovery";
        #endregion

        #region OrderDecline

        public const string CustomOrderDeclineNotification = "Custom.OrderDecline.Notification";
        public const string CustomCustomerOrderDeclineNotification = "Custom.Customer.OrderDecline.Notification";
        #endregion

        #region Wishlist

        public const string CustomWishlistToFriendMessage = "Custom.Wishlist.EmailAFriend";
        #endregion


        #region Abandoned Card
        public const string CustomSupportAbandonedCartNotification = "Custom.Support.AbandonedCart.Notification";
        #endregion

        #region Payment Issue
        public const string OrderTotalMismatchSupportNotification = "OrderTotalMismatch.AlertSupport";
        #endregion
    }
}
