using MWT.Nop.Core.Domain.PostDelivery;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Message;
using MWT.Nop.Core.Services.Orders;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Messages;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.PostDelivery
{
    public class PostDeliveryService : IPostDeliveryService
    {
        private readonly IRepository<PostDeliveryEmailJourney> _emailjourneyrepository;
        private readonly IRepository<PostDeliveryEmailJourneyReminder> _emailreminderService;
        private readonly IRepository<PostDeliveryEmailJourneyLog> _postDeliveryLog;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IMessageTemplateService _messageTemplate;
        private readonly ICustomWorkflowMessageService _workflowMessageService;
        private readonly IProductExtendedService _productservice;
        private readonly IOrderExtendedService _orderService;
        private readonly IOrderReportService _orderreportservice;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly ICategoryService _categoryService;

        public PostDeliveryService(
      IRepository<PostDeliveryEmailJourney> emailjourneyrepository,
      IRepository<PostDeliveryEmailJourneyReminder> emailreminderService,
      IRepository<PostDeliveryEmailJourneyLog> postDeliveryLog,
      ICustomerService customerService,
      IWorkContext workContext,
      IMessageTemplateService messageTemplate,
      ICustomWorkflowMessageService workflowMessageService,
            IProductExtendedService productservice,
            IOrderExtendedService orderService,
           IOrderReportService orderreportservice,
           ICustomSpecificationAttributeService specificationAttributeService,
           ICategoryService categoryService)

        {
            _emailjourneyrepository = emailjourneyrepository;
            _emailreminderService = emailreminderService;
            _postDeliveryLog = postDeliveryLog;
            _customerService = customerService;
            _workContext = workContext;
            _messageTemplate = messageTemplate;
            _workflowMessageService = workflowMessageService;
            _productservice = productservice;
            _orderService = orderService;
            _orderreportservice = orderreportservice;
            _specificationAttributeService = specificationAttributeService;
            _categoryService = categoryService;

        }

        #region EmailJourney
        public async Task InsertPostDeliveryEmailJourney(Order order)
        {

            if (!await _emailjourneyrepository.Table
          .AnyAsync(p => p.OrderId == order.Id))
            {
                PostDeliveryEmailJourney postPurchaseEmail = new PostDeliveryEmailJourney()
                {
                    OrderId = order.Id,
                    CreatedOn = DateTime.Now,
                    OrderDateTime = order.CreatedOnUtc.Date,
                    LastReminderId = 0,
                    UpdatedOn = DateTime.Now,
                    IsActive = true
                };
                await _emailjourneyrepository.InsertAsync(postPurchaseEmail);
            }
        }

        public async Task UpdatePostPurchaseEmailJourney(PostDeliveryEmailJourney postDelivery)
        {
            if (postDelivery != null)
            {
                await _emailjourneyrepository.UpdateAsync(postDelivery);
            }
        }

        public async Task<PostDeliveryEmailJourney> GetPostPurchaseEmailJourneyByOrderId(int orderId)
        {
            return await _emailjourneyrepository.Table.Where(pv => pv.OrderId == orderId).FirstOrDefaultAsync();
        }

        public async Task<IList<PostDeliveryEmailJourney>> GetRecordsForFirstPurchaseReminder(int reminderId, int days)
        {
            return await _emailjourneyrepository.Table
                .Where(x => x.OrderDateTime <= DateTime.Now.Date.AddDays(-days) && x.LastReminderId == reminderId)
                .ToListAsync();
        }

        public async Task<IList<PostDeliveryEmailJourney>> GetPurchaseJournalRecordsForReminder(int reminderId, int days)
        {
            var cutoffDate = DateTime.Now.Date.AddDays(-days);

            return await (from log in _postDeliveryLog.Table
                          join journey in _emailjourneyrepository.Table
                                         on log.PostDeliveryEmailJourneyId equals journey.Id
                          where log.SentOn <= cutoffDate
                                && journey.LastReminderId == reminderId && log.ReminderId == reminderId
                                && journey.IsActive == true
                          select journey)
                .Distinct()
                .ToListAsync();
        }


        public async Task<List<PostDeliveryQueueEmail>> GetPostDeliveryQueueEmailList()
        {
            List<PostDeliveryQueueEmail> lstPostPurchaseEmailQueueEmail = new List<PostDeliveryQueueEmail>();
            var orders = await _orderService.GetShippedOrdersForLastNDays(20);
            var reminders = await this.GetAllEmailReminders();
            int maxReminder = reminders.Where(r => r.IsDefault == true).Max(r => r.ReminderNo);
            foreach (var order in orders)
            {
                var postDeliveryJourney = await GetPostPurchaseEmailJourneyByOrderId(order.Id);
                if (postDeliveryJourney != null && postDeliveryJourney.LastReminderId == maxReminder)
                    continue;
                else
                {
                    int days = 0;
                    foreach (var reminder in reminders.Where(r => r.IsDefault == true))
                    {
                        days += reminder.ReminderDays;
                        if (postDeliveryJourney == null)
                        {
                            await this.InsertPostDeliveryEmailJourney(order);
                            lstPostPurchaseEmailQueueEmail.Add(new PostDeliveryQueueEmail()
                            {
                                OrderId = order.Id,
                                Email = order.CustomerEmail,
                                ReminderDate = order.CreatedOnUtc.AddDays(days),
                                ReminderNumber = reminder.ReminderNo,
                                IsActive = true
                            });
                            break;
                        }
                        else if (postDeliveryJourney.LastReminderId < reminder.ReminderNo)
                        {
                            lstPostPurchaseEmailQueueEmail.Add(new PostDeliveryQueueEmail()
                            {
                                OrderId = order.Id,
                                Email = order.CustomerEmail,
                                ReminderDate = order.CreatedOnUtc.AddDays(days),
                                ReminderNumber = reminder.ReminderNo,
                                DeactivatedRemarks = postDeliveryJourney.DeactivatedRemarks,
                                IsActive = postDeliveryJourney.IsActive
                            });
                            break;
                        }
                    }
                }

            }
            return lstPostPurchaseEmailQueueEmail;
        }


        #endregion

        #region EmailJourneyReninder
        public async Task<IList<PostDeliveryEmailJourneyReminder>> GetAllEmailReminders()
        {

            var list = await _emailreminderService.GetAllAsync(reminder =>
            {
                return from r in reminder
                       orderby r.ReminderNo
                       select r;
            }, cache => default);
            return list;
        }

        public async Task<PostDeliveryEmailJourneyReminder> GetEmailRemindersByEmailtype(string emailtype)
        {
            return await _emailreminderService.Table.FirstOrDefaultAsync(x => x.EmailType == emailtype);
        }
        #endregion


        #region Emailjouneylog
        public async Task InsertPostPurchaseEmailJourneyLog(PostDeliveryEmailJourneyLog postpurchaseLog)
        {
            if (postpurchaseLog != null)
            {
                await _postDeliveryLog.InsertAsync(postpurchaseLog);
            }
        }
        public async Task UpdatePostPurchaseEmailJourneyLog(PostDeliveryEmailJourneyLog postpurchaseLog)
        {
            if (postpurchaseLog != null)
            {
                await _postDeliveryLog.UpdateAsync(postpurchaseLog);
            }
        }



        #endregion


        public async Task SendReminderEmail(PostDeliveryEmailJourney emailjourney, PostDeliveryEmailJourneyReminder reminder)
        {
            List<Product> products = new List<Product>();
            var order = await _orderService.GetOrderByIdAsync(emailjourney.OrderId);
            var customer = await this._customerService.GetCustomerByIdAsync(order.CustomerId);
            int productId = 0;
            int categoryId = 0;
            string templatetype = string.Empty;
            if (customer != null)
            {
                if (reminder.ReminderNo == 1)
                {

                    var ordersItems = (await _orderService.GetOrderItemsAsync(emailjourney.OrderId)).OrderByDescending(oitem => oitem.UnitPriceExclTax);
                    foreach (var item in ordersItems)
                    {
                        var productIds = await _productservice.GetCollectionProductsByProductId1Async(item.ProductId);
                        if (productIds.Count > 0)
                        {
                            foreach (var id in productIds)
                            {
                                products.Add(await _productservice.GetProductByIdAsync(id));
                            }
                            templatetype = "complete your collection";
                            if (productId == 0)
                            {
                                productId = item.ProductId;
                            }
                        }
                    }

                    if (products.Count == 0)
                    {
                        foreach (var item in ordersItems)
                        {
                            var relatedProductIds = (await _productservice.GetCrossSellProductsByProductId1Async(item.ProductId)).Select(r => r.ProductId2);
                            if (relatedProductIds.Count() > 0)
                            {
                                foreach (var id in relatedProductIds)
                                {
                                    products.Add(await _productservice.GetProductByIdAsync(id));
                                }

                                templatetype = "you might also like";
                                if (productId == 0)
                                {
                                    productId = item.ProductId;
                                }
                            }
                        }
                    }

                    if (products.Count == 0)
                    {
                        products = new List<Product>();
                        foreach (var item in ordersItems)
                        {
                            int mainCategoryId = await _specificationAttributeService.GetMainCategoryOfProduct(item.ProductId);

                            if (mainCategoryId != 0)
                            {
                                var category = await _categoryService.GetCategoryByIdAsync(mainCategoryId);
                                if (category != null)
                                {
                                    if (productId == 0)
                                    {
                                        productId = item.ProductId;
                                    }
                                    int[] complementoryCategoryIds = new int[] { category.PrimaryCategoryId, category.SecondaryCategoryId };
                                    foreach (var complementoryCategoryId in complementoryCategoryIds.Where(c => c > 0))
                                    {
                                        var catogeryBestsellers = await _orderreportservice.BestSellersReportAsync(complementoryCategoryId);
                                        //var catogeryBestsellers = await _orderreportservice.BestSellersReportAsync(476);
                                        foreach (var bestseller in catogeryBestsellers)
                                        {
                                            products.Add(await _productservice.GetProductByIdAsync(bestseller.ProductId));
                                        }
                                        if (categoryId == 0)
                                        {
                                            categoryId = complementoryCategoryId;
                                        }
                                        if (products.Count >= 6) break;
                                    }
                                }
                            }

                        }

                        if (products.Count > 0) templatetype = "complementary categories";
                    }

                    if (templatetype == string.Empty)
                    {
                        emailjourney.UpdatedOn = DateTime.Now;
                        emailjourney.DeactivatedOn = DateTime.Now;
                        emailjourney.IsActive = false;
                        emailjourney.DeactivatedRemarks = "Email Journey stopped, Reason: There are no products available to send emails.";
                        await UpdatePostPurchaseEmailJourney(emailjourney);
                        return;
                    }



                    //string replacedHtml = "";
                    //if (templatetype == "complete your collection")
                    //    replacedHtml = await _messageTokenService.CustomProductAddHTMLEmailB(products);
                    //else if (templatetype == "you might also like")
                    //    replacedHtml = await _messageTokenService.CustomProductAddHTMLEmailC(products);
                    //else if (templatetype == "complementary categories")
                    //    replacedHtml = await _messageTokenService.CustomProductAddHTMLEmailD(products);

                    reminder = await GetEmailRemindersByEmailtype(templatetype);
                    if (reminder == null)
                    {
                        emailjourney.UpdatedOn = DateTime.Now;
                        emailjourney.DeactivatedOn = DateTime.Now;
                        emailjourney.IsActive = false;
                        emailjourney.DeactivatedRemarks = $"Email Journey stopped, Reason: Failed to Get Reminder for Template Type {templatetype}";
                        await UpdatePostPurchaseEmailJourney(emailjourney);
                        return;
                    }

                }

                var messageTemplate = await _messageTemplate.GetMessageTemplateByIdAsync(reminder.MessageTemplateId);
                if (messageTemplate == null)
                {
                    emailjourney.IsActive = false;
                    emailjourney.DeactivatedOn = DateTime.Now;
                    emailjourney.DeactivatedRemarks = $"Email Journey stopped, Reason: Message Template Not Found {reminder.MessageTemplateId}";
                    await UpdatePostPurchaseEmailJourney(emailjourney);
                    PostDeliveryEmailJourneyLog pLog = new PostDeliveryEmailJourneyLog()
                    {
                        PostDeliveryEmailJourneyId = emailjourney.Id,
                        ReminderHtml = string.Empty,
                        ReminderId = reminder.ReminderNo,
                        Comments = $"Email Journey stopped, Reason: Message Template Not Found {reminder.MessageTemplateId}",
                        CreatedOn = DateTime.Now
                    };
                    await InsertPostPurchaseEmailJourneyLog(pLog);
                    return;
                }



                (bool response, string body) = await _workflowMessageService.SendPurchaseJourneyNotificationAsync(customer, products, productId, categoryId, templatetype, reminder.MessageTemplateId, reminder.UtmParameters, (await _workContext.GetWorkingLanguageAsync())?.Id ?? 0);
                if (response)
                {
                    emailjourney.LastReminderId = reminder.ReminderNo;
                    emailjourney.UpdatedOn = DateTime.Now;
                    await UpdatePostPurchaseEmailJourney(emailjourney);

                    PostDeliveryEmailJourneyLog pLog = new PostDeliveryEmailJourneyLog()
                    {
                        PostDeliveryEmailJourneyId = emailjourney.Id,
                        ReminderHtml = body,
                        ReminderId = reminder.ReminderNo,
                        SentOn = DateTime.Now.Date.AddDays(0),
                        CreatedOn = DateTime.Now
                    };
                    await InsertPostPurchaseEmailJourneyLog(pLog);
                }


            }
            else
            {
                emailjourney.IsActive = false;
                emailjourney.DeactivatedOn = DateTime.Now;
                emailjourney.DeactivatedRemarks = $"Email Journey stopped, Reason: Customer Not Found {order.CustomerId}";
                await UpdatePostPurchaseEmailJourney(emailjourney);
                PostDeliveryEmailJourneyLog pLog = new PostDeliveryEmailJourneyLog()
                {
                    PostDeliveryEmailJourneyId = emailjourney.Id,
                    ReminderHtml = string.Empty,
                    ReminderId = reminder.ReminderNo,
                    CreatedOn = DateTime.Now,
                    Comments = $"Email Journey stopped, Reason: Customer Not Found {order.CustomerId}"
                };
                await InsertPostPurchaseEmailJourneyLog(pLog);

            }

        }
    }
}
