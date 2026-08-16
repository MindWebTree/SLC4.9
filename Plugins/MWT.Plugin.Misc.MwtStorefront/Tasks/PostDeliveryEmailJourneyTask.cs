using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Configuration;
using Nop.Services.ScheduleTasks;
using MWT.Nop.Core.Services.PostDelivery;
using Nop.Services.Customizations.CustomOrders;
using MWT.Nop.Core.Services.Orders;
using Nop.Core.Domain.Logging;
using MWT.Nop.Core.Domain.CustomOrders;
using MWT.Nop.Core.Domain.PostDelivery;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public partial class PostDeliveryEmailJourneyTask : IScheduleTask
    {
        private readonly ILogger _loggerService;
        bool enableLog = true;
        private readonly IOrderExtendedService _orderService;
        private readonly IPostDeliveryService _journeyservice;
        private readonly IProductService _productService;
        private readonly ICustomOrderService _customOrderService;
        private int[] workingHours = { 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
        private ISettingService _settingService;

        public PostDeliveryEmailJourneyTask(IOrderExtendedService orderService, IPostDeliveryService journeyservice, ILogger loggerService, IProductService productService,
            ICustomOrderService customOrderService, ISettingService settingService)
        {
            _orderService = orderService;
            _journeyservice = journeyservice;
            _loggerService = loggerService;
            _productService = productService;
            _customOrderService = customOrderService;
            _settingService= settingService;
        }
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            enableLog = await _settingService.GetSettingByKeyAsync<bool>("PostDelivery.Offer.Task.Enable.Log");
            bool isSandboxEnviromentEnabled = await _settingService.GetSettingByKeyAsync<bool>("PostDelivery.Offer.Task.Enable.SandboxEnviroment");
            if (workingHours.Contains(DateTime.Now.Hour) || isSandboxEnviromentEnabled)
            {
                await this.InsertLog("Post Delivery Journey", "Task Started", LogLevel.Information);
                var pastSevenDaysOrders = await _orderService.GetShippedOrdersForLastNDays(11);
                var orderTypes = await _customOrderService.GetOrderTypes();
                foreach (var order in pastSevenDaysOrders)
                {
                    var customOrder = await _customOrderService.GetByOrderNumber(order.Id);
                    if (customOrder == null || (customOrder.ParentOrderID == 0 && (customOrder.SubOrderTypeId ?? 0) == 0
                        && (
                        (orderTypes.Where(o => o.Id == customOrder.OrderTypeId).FirstOrDefault()?.Name ?? string.Empty) == OrderTypes.CustomOrder.ToString()
                        ||
                        (orderTypes.Where(o => o.Id == customOrder.OrderTypeId).FirstOrDefault()?.Name ?? string.Empty) == OrderTypes.AlreadyPaid.ToString())))
                    {
                        await _journeyservice.InsertPostDeliveryEmailJourney(order);
                    }
                }

                var allReminders = await _journeyservice.GetAllEmailReminders();
                foreach (var reminder in allReminders.Where(x => x.IsDefault == true))
                {
                    IList<PostDeliveryEmailJourney> emailjourneys = new List<PostDeliveryEmailJourney>();
                    if (reminder.ReminderNo == 1)
                    {
                        emailjourneys = await _journeyservice.GetRecordsForFirstPurchaseReminder((reminder.ReminderNo - 1), reminder.ReminderDays);
                    }
                    else
                    {
                        emailjourneys = await _journeyservice.GetPurchaseJournalRecordsForReminder(reminder.ReminderNo - 1, reminder.ReminderDays);
                    }
                    foreach (var journey in emailjourneys)
                    {
                        await _journeyservice.SendReminderEmail(journey, reminder);
                    }

                }
            }

        }

        public async System.Threading.Tasks.Task InsertLog(string shortMessage, string message, LogLevel logLevel, bool hardEnableLog = false)
        {
            if (enableLog || hardEnableLog)
            {
                await _loggerService.InsertLogAsync(logLevel, shortMessage, message);
            }
        }
    }
}