using MWT.Nop.Core.Domain.PostPurchase;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.Message;
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

namespace MWT.Nop.Core.Services.PostPurchase
{
    public class PostPurchaseService : IPostPurchaseService
    {
        private readonly IRepository<PostPurchaseEmailJourney> _emailjourneyrepository;
        private readonly IRepository<PostPurchaseEmailJourneyReminder> _emailreminderService;
        private readonly IRepository<PostPurchaseEmailJourneyLog> _postpurchaseLog;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IMessageTemplateService _messageTemplate;
        private readonly ICustomWorkflowMessageService _workflowMessageService;
        private readonly IProductExtendedService _productservice;
        private readonly IOrderService _orderservice;
        private readonly IOrderReportService _orderreportservice;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly ICategoryService _categoryService;

        public PostPurchaseService(
      IRepository<PostPurchaseEmailJourney> emailjourneyrepository,
      IRepository<PostPurchaseEmailJourneyReminder> emailreminderService,
      IRepository<PostPurchaseEmailJourneyLog> postpurchaseLog,
      ICustomerService customerService,
      IWorkContext workContext,
      IMessageTemplateService messageTemplate,
      ICustomWorkflowMessageService workflowMessageService,
            IProductExtendedService productservice,
            IOrderService orderservice,
           IOrderReportService orderreportservice,
           ICustomSpecificationAttributeService specificationAttributeService,
           ICategoryService categoryService)

        {
            _emailjourneyrepository = emailjourneyrepository;
            _emailreminderService = emailreminderService;
            _postpurchaseLog = postpurchaseLog;
            _customerService = customerService;
            _workContext = workContext;
            _messageTemplate = messageTemplate;
            _workflowMessageService = workflowMessageService;
            _productservice = productservice;
            _orderservice = orderservice;
            _orderreportservice = orderreportservice;
            _specificationAttributeService = specificationAttributeService;
            _categoryService = categoryService;

        }

        #region EmailJourney
        public async Task InsertPostPurchaseEmailJourney(Order order)
        {

            if (!await _emailjourneyrepository.Table
          .AnyAsync(p => p.OrderId == order.Id))
            {
                PostPurchaseEmailJourney postPurchaseEmail = new PostPurchaseEmailJourney()
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

        public async Task UpdatePostPurchaseEmailJourney(PostPurchaseEmailJourney postpurchase)
        {
            if (postpurchase != null)
            {
                await _emailjourneyrepository.UpdateAsync(postpurchase);
            }
        }
        public async Task<IList<PostPurchaseEmailJourney>> GetRecordsForFirstPurchaseReminder(int reminderId, int days)
        {
            return await _emailjourneyrepository.Table
                .Where(x => x.OrderDateTime <= DateTime.Now.Date.AddDays(-days) && x.LastReminderId == reminderId)
                .ToListAsync();
        }

        public async Task<IList<PostPurchaseEmailJourney>> GetPurchaseJournalRecordsForReminder(int reminderId, int days)
        {
            var cutoffDate = DateTime.Now.Date.AddDays(-days);

            return await (from log in _postpurchaseLog.Table
                          join journey in _emailjourneyrepository.Table
                                         on log.PostPurchaseEmailJourneyId equals journey.Id
                          where log.SentOn <= cutoffDate
                                && journey.LastReminderId == reminderId && log.ReminderId == reminderId
                                && journey.IsActive == true
                          select journey)
                .Distinct()
                .ToListAsync();
        }

        #endregion

        #region EmailJourneyReninder
        public async Task<IList<PostPurchaseEmailJourneyReminder>> GetAllEmailReminders()
        {

            var list = await _emailreminderService.GetAllAsync(reminder =>
            {
                return from r in reminder
                       orderby r.ReminderNo
                       select r;
            }, cache => default);
            return list;
        }

        public async Task<PostPurchaseEmailJourneyReminder> GetEmailRemindersByEmailtype(string emailtype)
        {
            return await _emailreminderService.Table.FirstOrDefaultAsync(x => x.EmailType == emailtype);
        }
        #endregion


        #region Emailjouneylog
        public async Task InsertPostPurchaseEmailJourneyLog(PostPurchaseEmailJourneyLog postpurchaseLog)
        {
            if (postpurchaseLog != null)
            {
                await _postpurchaseLog.InsertAsync(postpurchaseLog);
            }
        }
        public async Task UpdatePostPurchaseEmailJourneyLog(PostPurchaseEmailJourneyLog postpurchaseLog)
        {
            if (postpurchaseLog != null)
            {
                await _postpurchaseLog.UpdateAsync(postpurchaseLog);
            }
        }



        #endregion


        public async Task SendReminderEmail(PostPurchaseEmailJourney emailjourney, PostPurchaseEmailJourneyReminder reminder)
        {
            List<Product> products = new List<Product>();
            var order = await _orderservice.GetOrderByIdAsync(emailjourney.OrderId);
            var customer = await this._customerService.GetCustomerByIdAsync(order.CustomerId);
            int productId = 0;
            int categoryId = 0;
            string templatetype = string.Empty;
            if (customer != null)
            {
                if (reminder.ReminderNo == 2)
                {

                    var ordersItems = await _orderservice.GetOrderItemsAsync(emailjourney.OrderId);
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
                    PostPurchaseEmailJourneyLog pLog = new PostPurchaseEmailJourneyLog()
                    {
                        PostPurchaseEmailJourneyId = emailjourney.Id,
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

                    PostPurchaseEmailJourneyLog pLog = new PostPurchaseEmailJourneyLog()
                    {
                        PostPurchaseEmailJourneyId = emailjourney.Id,
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
                PostPurchaseEmailJourneyLog pLog = new PostPurchaseEmailJourneyLog()
                {
                    PostPurchaseEmailJourneyId = emailjourney.Id,
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
