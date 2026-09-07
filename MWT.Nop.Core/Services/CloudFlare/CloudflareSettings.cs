using Nop.Core.Configuration;

namespace MWT.Nop.Core.Services.CloudFlare
{
    public class  CloudflareSettings: ISettings
    {
        public string Token { get;set; }
        public string EndPoint { get; set; }

        public string ImageSizes { get;set; }
    }
}
