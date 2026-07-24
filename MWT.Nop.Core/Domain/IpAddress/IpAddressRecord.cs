using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.IpAddress
{
    public class IpAddressRecord : BaseEntity
    {
        public string IpAddress { get; set; }
        public string Region { get; set; }
        public string Time_Zone { get; set; }
        public string Zip_Code { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Country_Code { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
