using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.CloudFlare
{
    public partial interface ICloudflareService
    {
        Task ClearCacheOfFiles(IList<string> files);
    }
}
