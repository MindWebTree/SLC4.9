using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.CloudFlare
{
    public class  CloudflareSettings: ISettings
    {
        public string Token { get;set; }
        public string EndPoint { get; set; }

        public string ImageSizes { get;set; }
    }
}
