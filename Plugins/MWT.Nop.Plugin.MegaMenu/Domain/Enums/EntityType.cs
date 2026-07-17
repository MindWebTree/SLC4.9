using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Domain.Enums
{
    public enum EntityType
    {
        Attachment = 5,
        Tab = 10, // 0x0000000A
        Slider = 15, // 0x0000000F
        SEO = 20, // 0x00000014
        HtmlWidget = 25, // 0x00000019
        ProductRibbon = 30, // 0x0000001E
        Carousel = 35, // 0x00000023
        UltimateCarousel = 37, // 0x00000025
        SaleCampaign = 40, // 0x00000028
        SaleOfTheDayOffer = 45, // 0x0000002D
        GroupItem = 50, // 0x00000032
        SocialFeed = 55, // 0x00000037
        Menu = 60, // 0x0000003C
        Locator = 65, // 0x00000041
    }
}
