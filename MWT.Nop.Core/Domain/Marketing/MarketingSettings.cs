using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Domain.Marketing
{
    public class MarketingSettings : ISettings
    {
        public bool EnableBuyMoreSaveMoreDiscount { get; set; }
        public string BuyMoreSaveMoreDiscountConfiguration { get; set; }
        public bool ShowBuyMoreSaveMoreBanner { get; set; }
        public string HeaderStrip { get; set; }
        public string LimitedOffer { get; set; }
        public string BuyMoreSaveMoreContent { get; set; }
        public bool EnableEmailExclusiveOffer { get; set; }
    }
}
