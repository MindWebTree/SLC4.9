using MWT.Nop.Core.Domain.Zoho;
using MWT.Nop.Core.Services.Zoho;
using Nop.Core.Domain.Customers;
namespace MWT.Nop.Core.Service.Zoho
{
    public partial interface IZohoService
    {
        Task<string> SaveLead(ZohoDto obj, string zohoLeadId = "");
        Task SaveContact(QueuedZohoCustomer queuedCustomer);
        Task<ZohoDto> GetContactDetailsFromZoho(string email, string phone);
        Task<List<QueuedZohoCustomer>> QueuedZohoCustomerListAsync();
        Task InsertQueuedZohoCustomerAsync(int customerId);
        Task UpdateQueuedZohoCustomerAsync(QueuedZohoCustomer queuedZohoCustomer);
        Task<string> CreateUpdateOrderContactPotential(int orderId, string zohoPotentialId, Customer customer, string orderStatus, string description, string ipAddress, string glclidCookie, decimal orderTotal,
            int createdBy, string orderLink,
            bool isCustomOrder = false);
    }
}
