using Microsoft.AspNetCore.Http;
using MWT.Nop.Core.Domain.Mailchimp;
using MWT.Nop.Core.Services.MailChimp;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Customers;
using Nop.Services.Events;

namespace Nop.Services.Customizations.Orders.Consumer
{
    public partial class ShoppingCartItemCacheEventConsumer : IConsumer<EntityInsertedEvent<ShoppingCartItem>>
    {
        #region Fields

        private readonly IQueuedMailChimpCartSignUpService _queuedMailChimpCartSignUpService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public ShoppingCartItemCacheEventConsumer(IQueuedMailChimpCartSignUpService queuedMailChimpCartSignUpService,
            IWorkContext workContext, ICustomerService customerService, IHttpContextAccessor httpContextAccessor)
        {
            this._queuedMailChimpCartSignUpService = queuedMailChimpCartSignUpService;
            this._workContext = workContext;
            this._customerService = customerService;
            this._httpContextAccessor = httpContextAccessor;
        }

        #endregion
        public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var email = customer.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    string ipAddress = "";
                    try
                    {
                        ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                    }
                    catch { }
                    var name = (await _customerService.GetCustomerFullNameAsync(customer)) ?? "";
                    var parts = name.Split(' ');
                    string firsName = parts[0];
                    string lastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "";
                    string url = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                    string userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
                    await this._queuedMailChimpCartSignUpService.InsertAsync
                              (new QueuedMailChimpCartSignUp()
                              {
                                  CreatedOn = DateTime.UtcNow,
                                  Email = email,
                                  FirstName = firsName,
                                  LastName = lastName,
                                  IpAddress = ipAddress,
                                  IsProcessed = false,
                                  NoOfTries = 0,
                                  ProductId = eventMessage.Entity.ProductId,
                                  ProcessedOn = null,
                                  ShoppingCartTypeId = eventMessage.Entity.ShoppingCartTypeId,
                                  UpdatedOn = DateTime.UtcNow,
                                  Url = url,
                                  UserAgent = userAgent
                              });
                }

            }
            catch
            {

            }
            
        }
    }
}
