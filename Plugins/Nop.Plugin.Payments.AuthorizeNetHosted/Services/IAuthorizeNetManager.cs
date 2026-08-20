using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using Microsoft.AspNetCore;
using Nop.Plugin.Payments.AuthorizeNetHosted.Helpers;
using Nop.Plugin.Payments.AuthorizeNetHosted.Models;
using Nop.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.AuthorizeNetHosted.Services
{
    public interface IAuthorizeNetManager
    {
        Task<(decimal orderTotal, string currencyCode)> GetOrderTotalAsync(int currencyId, decimal orderTotal);
        string GetLibraryUrl(bool useSandbox);
        merchantAuthenticationType PrepareAuthorizeNet();
        Task<createTransactionResponse> GetApiResponseAsync(createTransactionController controller, IList<string> errors);
        Task WebhookPaymentEventAsync(paymentEvent paymentEvent);
        Task<getTransactionDetailsResponse> GetTransactionDetailsAsync(string transactionId);

        Task<string> FindTransactionIdByInvoiceAsync(string expectedInvoiceNumber, int customerId);
        Task<createTransactionResponse> CreateTransactionAsync(
             string dataValue, string dataDescriptor,
             ProcessPaymentRequest processPaymentRequest, IList<string> errors);

        Task<string> GetHostedFormToken(ProcessPaymentRequest paymentRequest, int invoiceId, dynamic additionalData);

        Task<(string customerProfileId, string paymentProfileId)> CreateProfileFromTransaction(
    string transId, Nop.Core.Domain.Customers.Customer customer, string existingProfileId);
    }
}