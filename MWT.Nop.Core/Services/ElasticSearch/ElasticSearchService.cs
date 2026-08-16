using Nop.Core.Domain.Logging;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using System.Text;

namespace MWT.Nop.Core.Services.ElasticSearch
{
    public partial class ElasticSearchService : IElasticSearchService
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly HttpClient _httpClient;
        private readonly ILogger _loggerService;

        #endregion

        #region Ctor

        public ElasticSearchService
            (ISettingService settingService,
             HttpClient httpClient,
             ILogger loggerService)
        {
            this._settingService = settingService;
            this._httpClient = httpClient;
            this._loggerService = loggerService;
        }

        #endregion

        #region Methods

        public async Task<string> InsertDataToElasticSearch(string json, string index, string ID)
        {
            string responseFromServer = string.Empty;
            var result = string.Empty;
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync((await _settingService.GetSettingByKeyAsync<string>("Es_Host")) + index + "/" + ID, content);
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception exp)
            {
                await _loggerService.InsertLogAsync(LogLevel.Error, "Elastic Search",
                    $"Error for Index {index} in Id {ID} on POST Method."
                    );
            }
            return result;
        }
        public async Task<string> DeleteEntity(string json, string index, string ID)
        {
            string responseFromServer = string.Empty;
            var result = string.Empty;
            try
            {
                var response = await _httpClient.DeleteAsync((await _settingService.GetSettingByKeyAsync<string>("Es_Host")) + index + "/" + ID);
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception exp)
            {
                await _loggerService.InsertLogAsync(LogLevel.Error, "Elastic Search",
                  $"Error for Index {index} in Id {ID} on DELETE Method."
                  );
            }
            return result;
        }



        #endregion
    }
}
