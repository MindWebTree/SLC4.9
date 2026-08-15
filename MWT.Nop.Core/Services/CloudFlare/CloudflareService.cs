using Newtonsoft.Json;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;
using Org.BouncyCastle.Crypto.Engines;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.CloudFlare
{
    public partial class CloudflareService : ICloudflareService
    {
        #region fields

        private readonly HttpClient _httpClient;
        private readonly ILogger _loggerService;
        CloudflareSettings _cloudflareSettings;

        #endregion

        #region Ctor

        public CloudflareService(HttpClient httpClient, ILogger loggerService, CloudflareSettings cloudflareSettings)
        {
            _httpClient = httpClient;
            _loggerService = loggerService;
            _cloudflareSettings = cloudflareSettings;
        }

        #endregion

        #region Methods
        public async Task ClearCacheOfFiles(IList<string> files)
        {
            if (string.IsNullOrWhiteSpace(_cloudflareSettings.Token) || string.IsNullOrWhiteSpace(_cloudflareSettings.EndPoint))
            {
                await _loggerService.InsertLogAsync(LogLevel.Error, "Cloudflare Failed to clear cache ",
                    "Token or EndPoint missed");
            }
            else
            {
                try
                {
                    var body = new
                    {
                        files = files
                    };
         
                    var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                    _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer  " + _cloudflareSettings.Token);
                    var response = await _httpClient.PostAsync(_cloudflareSettings.EndPoint, content);
                    response.EnsureSuccessStatusCode();
             
                    await _loggerService.InsertLogAsync(LogLevel.Information, "Cloudflare cache cleared", string.Join(',', files));

                }
                catch (Exception ex)
                {
                    await _loggerService.InsertLogAsync(LogLevel.Error, "Cloudflare Failed to clear cache",
                  ex.Message);
                }
            }
        }

        #endregion
    }
}
