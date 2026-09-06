using Nop.Core.Domain.Logging;
using Nop.Core.Http;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace MWT.Nop.Core.Services.FeedBack
{
    public partial class FeedbackService : IFeedbackService
    {
        #region Fields

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        #endregion


        #region Ctor
        public FeedbackService(IHttpClientFactory httpClientFactory,
            ISettingService settingService,
            ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _settingService = settingService;
            _logger = logger;
        }

        #endregion

        #region Methods
        public async Task<string> GetProductFeedBacks(int pageSize, string sku, int mainCategoryId)
        {
            string response = "";
            var url = await _settingService.GetSettingByKeyAsync<string>("Products.Feedback.Link");
            try
            {

                if (string.IsNullOrEmpty(url))
                    await _logger.InsertLogAsync(LogLevel.Error,
                        "Products Review Block", "Products.Feedback.Link setting is empty");
                else
                {

                    var _httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                    response = await _httpClient.GetStringAsync($"{url}/{pageSize}/{sku}");
                }
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Products Review Block",
                      $"Failed to get Reviews for Sku {sku}" + ex.Message + $"{url}/{pageSize}/{sku}/{mainCategoryId}");
            }
            return response;
        }

        #endregion
    }
}
