using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.IPLite
{
    public partial interface IIPLiteService
    {
         Task<(string CountryCode, string Country, string Region, string City, string Longitude, string Latitude, string ZipCode, string TimeZone)> getDetail(string IPAddress);
    }
}
