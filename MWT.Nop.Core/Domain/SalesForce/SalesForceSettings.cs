using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.SalesForce
{
    public class SalesForceSettings : ISettings
    {
        public string Client_Id { get; set; }
        public string Client_Secret { get; set; }
        public string Account_ID { get; set; }
        public string AuthenticationUrl { get; set; }
        public string RestApiUrl { get; set; }
        public string CustomerApiUrl { get; set; }
        public string SftUserName { get; set; }
        public int SftPort{ get; set; }
        public string SftPassword { get; set; }
        public string SftHost { get; set; }
    }
}
