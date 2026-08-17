using MWT.Nop.Core.Domain.Mailchimp;
using MWT.Nop.Core.Services.MailChimp;
using Newtonsoft.Json;
using Nop.Core.Domain.Logging;
using Nop.Core.Http;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using System.Net;

namespace MWT.Plugin.Misc.MwtStorefront.Tasks
{
    public partial class MailchimpSegmentSyncTask : IScheduleTask
    {
        #region Fields
        private string listID = "";
        private string apiKey = "";
        private string host = "";
        private readonly ISettingService _settingService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly IMailchimpSegmentsService _mailchimpSegmentsService;
        #endregion

        #region Ctor

        public MailchimpSegmentSyncTask(ISettingService settingService,
            IHttpClientFactory httpClientFactory, ILogger logger,
            IMailchimpSegmentsService mailchimpSegmentsService)
        {
            _settingService = settingService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _mailchimpSegmentsService = mailchimpSegmentsService;
            listID = (_settingService.GetSettingByKeyAsync<string>("MailChimp.ItemStock.ListId")).Result;
            apiKey = (_settingService.GetSettingByKeyAsync<string>("MailChimp.ApiKey")).Result;
            host = (_settingService.GetSettingByKeyAsync<string>("MailChimp.Api.EndPoint")).Result;
        }

        #endregion

        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {
            bool loop = true;
            string content = "";
            var segments = await this._mailchimpSegmentsService.Segments();
            var count = 50;
            var offset = 0;
            do
            {
                content = "";
                var httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("Authorization", "apikey " + apiKey);
                ;
                var response = await httpClient.GetAsync(host + "/lists/" + listID + "/segments/?count=" + count + "&offset=" + offset);
                content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    dynamic result = JsonConvert.DeserializeObject(content);
                    int totalItems = Convert.ToInt32(result.total_items);
                    foreach (var item in result.segments)
                    {
                        string segmentName = (string)item.name;
                        string segmentID = (from segment in segments
                                            where segment.NewsLetterForm.Equals(segmentName, StringComparison.InvariantCultureIgnoreCase)
                                            select segment.SegmentID.ToString()).FirstOrDefault();
                        if (string.IsNullOrEmpty(segmentID))
                        {
                            await _mailchimpSegmentsService.Insert(new MailchimpSegments()
                            {
                                CreatedBy = "ScheduleTask",
                                CreatedOn = DateTime.UtcNow,
                                Listid = listID,
                                ListName = "",
                                NewsLetterForm = "",
                                SegmentID = Convert.ToInt32(result.id),
                                SegmentName = segmentName,
                                UpdatedBY = "ScheduleTask",
                                UpdatedON = DateTime.UtcNow
                            });
                        }
                    }

                    if (offset > totalItems)
                        loop = false;
                    offset = offset + 50;
                }
                else
                {
                    loop = false;
                    await _logger.InsertLogAsync(LogLevel.Error, "Mailchimp Segments sync Task", content);
                }
            } while (loop);

        }
    }
}
